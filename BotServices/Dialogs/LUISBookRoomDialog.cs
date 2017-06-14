using BotServices.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotServices.Dialogs
{
    [LuisModel("01964138-f32c-401c-aaec-22e62b63573a", "a64a5818c24a4e8cafd20db1354deea3")]
    [Serializable]
    public class BookRoomDialog : LuisDialog<object>
    {
        [field: NonSerialized()]
        protected Activity _message;

        private readonly string[] introductions =
            {
                "Nice to talk with you {0}. How can I help you today?",
                "Hello {0}! How can I help you?"
            };

        private readonly string[] greetings =
            {
                "Hi! {0} Welcome back. How can I help you today?",
                "Happy to know about you {0}. How can I help you today?",
                "Hi There {0}. what do you need?"
            };

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _message = (Activity)await item;
            await base.MessageReceived(context, item);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, didn't understand. Try again");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greatings")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            var userName = string.Empty;
            userName = context.Activity.From.Name;

            if (CheckBooksforUser(context))
            {
                await context.PostAsync(string.Format(GetRandom(this.greetings), userName));
            }
            else
            {
                await context.PostAsync("I´m T-Bot. I manage the reservation of rooms.");
                await context.PostAsync(string.Format(GetRandom(this.introductions), userName));
            }

            context.Wait(this.MessageReceived);
        }

        //private async Task GetNameAsync(IDialogContext context, IAwaitable<string> result)
        //{
        //    var userName = string.Empty;
        //    context.UserData.TryGetValue<string>("Name", out userName);
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        await context.PostAsync("Hi, I am a reservation manager for meeting rooms Bot." + "\n\r" + "How can I help you today?");
        //        context.UserData.SetValue<bool>("Name", true);
        //        context.UserData.SetValue<bool>("GetName", true);
        //    }
        //    else
        //    {
        //        await context.PostAsync("How can I help you?");
        //    }
        //}
        private static string GetRandom(IReadOnlyList<string> collection)
        {
            var random = new Random();

            var next = random.Next(0, collection.Count);

            return collection[next];
        }

        [LuisIntent("BookMeetingRoom")]
        public async Task NewBook(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var state = new BookMeetingRoomForm();
            var entities = new List<EntityRecommendation>(result.Entities);

            if (entities.Count > 0)
            {
                FillEntityNewBookFromLuis(result, out state);
                entities = null;
            }

            var formDialog = new FormDialog<BookMeetingRoomForm>(state, BookMeetingRoomForm.BuildForm
                      , FormOptions.PromptInStart, entities);

            context.Call(formDialog, Callback);
        }
        private void FillEntityNewBookFromLuis(LuisResult result, out BookMeetingRoomForm entity)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            entity = new BookMeetingRoomForm();

            foreach (var item in entities)
            {
                if (item.Type.Contains("builtin.datetimeV2.timerange"))
                {
                    foreach (var vals in item.Resolution.Values)
                    {
                        entity.StartTime = (DateTime)((JArray)vals).First.SelectToken("start");
                        entity.EndTime = (DateTime)((JArray)vals).First.SelectToken("end");
                    }
                }

                if (item.Type.Contains("builtin.datetimeV2.date"))
                {
                    foreach (var vals in item.Resolution.Values)
                    {
                        entity.StartDate = (DateTime)((JArray)vals).First.SelectToken("value");
                    }
                }

                if (item.Type.Contains("TypeRoom"))
                {
                    entity.TypeRoom = TypeMeetingRoom.without_projector;// item.Entity.ToString();
                }

                if (item.Type.Contains("Attendant") || item.Type.Contains("number"))
                {
                    entity.Attendant = int.Parse(item.Entity);
                }
            }
        }
        [LuisIntent("CancelMeetingRoom")]
        public async Task Cancel(IDialogContext context, LuisResult result)
        {
            var entity = new CancelMeetingRoomForm();
            var entities = new List<EntityRecommendation>(result.Entities);

            await context.PostAsync("Here you have all!!!");

            await PostMeetingRoomCarousel(context, "PostBack");

            PromptDialog.Text(context, this.SaveChangesfromCancelBooking, "Tell me the meeting room you want to cancel.");
        }
        private void FillEntityShowBookingFromLuis(LuisResult result, out ViewMeetingRoomForm entityShowRoom)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            entityShowRoom = new ViewMeetingRoomForm();

            foreach (var entity in entities)
            {
                if (entity.Type.Contains("NameRoom"))
                {
                    entityShowRoom.NameRoom = entity.Entity.ToString();
                }

                if (entity.Type.Contains("builtin.datetimeV2.date"))
                {
                    entityShowRoom.StartDate = DateTime.Parse(entity.Entity).Date;
                }
            }
        }
        [LuisIntent("ShowReservation")]
        public async Task FindBook(IDialogContext context, LuisResult result)
        {
            if (CheckBooksforUser(context))
            {
                await PostMeetingRoomCarousel(context, "View");
            }
            else
            {
                await context.PostAsync("Has no reservations!!!");
            }
        }

        private bool CheckBooksforUser(IDialogContext context)
        {
            var db = new Model.EntityModelContainer();
            var id = context.Activity.From.Name;

            var message = string.Empty;

            var item = db.Bookings.FirstOrDefault(b => b.EmplyeeId == id && b.IsCancel == false);

            if (item == null)
                return false;
            else
                return true;
        }
        private async Task Callback(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
        }

        private static async Task PostMeetingRoomCarousel(IDialogContext context, string typeAction)
        {
            var db = new Model.EntityModelContainer();
            var user = context.Activity.From.Name;
            var reply = context.MakeMessage();
            var TypeAction = string.Empty;


            if (typeAction == "View")
                TypeAction = ActionTypes.ShowImage;
            else
                TypeAction = ActionTypes.PostBack;


            reply.AttachmentLayout = "carousel";

            var contactQuery = from Bookings in db.Bookings
                               where Bookings.EmplyeeId == user && Bookings.IsCancel == false
                               select Bookings;

            // Iterate through the results of the parameterized query.
            foreach (var list in contactQuery)
            {
                var heroCard = new HeroCard();

                heroCard.Buttons = new List<CardAction>
                                {
                                    new CardAction
                                        {
                                            // Type, title and value are required!
                                            Type = TypeAction,
                                            Title = $"{list.Room.NameRoom} - {list.StartDate.ToShortDateString()} - {list.StartTime.ToShortTimeString()}",
                                            Value = $"{list.Id}",
                                        }
                                };
                heroCard.Title = string.Empty;
                heroCard.Images = new List<CardImage>
                                 {
                                     new CardImage
                                         {
                                             Url = GetImageOfMeetingRoom(list.Room.NameRoom)
                                         }
                                 };

                reply.Attachments.Add(heroCard.ToAttachment());
            }

            await context.PostAsync(reply);
        }

        private static string GetImageOfMeetingRoom(string nameRoom)
        {

            if (nameRoom == "Pozuelo")
            {
                return @"http://botservices20170522052619.azurewebsites.net/Images/Pozuelo.jpg";
            }

            else if (nameRoom == "Guadalix")
            {
                return @"http://botservices20170522052619.azurewebsites.net/Images/Guadalix.jpg";
            }

            else if (nameRoom == "Navacerrada")
            {
                return @"http://botservices20170522052619.azurewebsites.net/Images/Navacerrada.jpg";
            }

            else
            {
                return @"http://botservices20170522052619.azurewebsites.net/Images/Torrelodones.jpg";
            }
        }
        private async Task SaveChangesfromCancelBooking(IDialogContext context, IAwaitable<string> result)
        {
            using (var db = new Model.EntityModelContainer())
            {
                var id = 0;

                id = int.Parse(result.GetAwaiter().GetResult());

                var itemToRemove = db.Bookings.SingleOrDefault(b => b.Id == id);

                itemToRemove.IsCancel = true;

                db.SaveChanges();
            }

            await context.PostAsync("Great....Cancelled Reservation.");
        }
    }
}

