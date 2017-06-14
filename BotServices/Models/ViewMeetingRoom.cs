using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BotServices.Models
{
    [Serializable]
    public class ViewMeetingRoomForm
    {
        [Prompt("What day do you want to look for?: {||}", AllowDefault = BoolDefault.True)]
        [Describe("Date of the Reservation ")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date (ex: 2/14/2017)")]
        [Required]
        public DateTime StartDate { get; set; }
        [Prompt("Tell me the name of the meeting room: {||}", AllowDefault = BoolDefault.True)]
        [Required]
        [Describe("Name of the meeting room ")]
        public string NameRoom { get; set; }

        public static IForm<ViewMeetingRoomForm> BuildForm()
        {
            var result = false;

            OnCompletionAsyncDelegate<ViewMeetingRoomForm> processOrder = async (context, state) =>
            {
                ShowBookings(context, state, out result);

                if (result)
                {
                    //await context.PostAsync("Thanks for using our service");
                    //await context.PostAsync("Bye!! ;-)");
                }
                else
                {
                    await context.PostAsync("Has no reservation!!!!!");
                    //await context.PostAsync("Thanks for using our service");
                    //await context.PostAsync("Bye!! ;-)");
                }
            };

            return new FormBuilder<ViewMeetingRoomForm>()
                .Message("Let's see that we found...")
                .Field(nameof(StartDate))
                .Field(nameof(NameRoom))
                .Build();
        }

        private static void ShowBookings(IDialogContext context, ViewMeetingRoomForm entity, out bool result)
        {
            var db = new Model.EntityModelContainer();
            result = false;
            var user = context.Activity.From.Name;

            IQueryable<Model.Booking> contactQuery = null;
            // Iterate through the results of the parameterized query.
            foreach (var list in contactQuery)
            {
                var cont = 0;
                var nameRoom = db.RoomSet.FirstOrDefault(b => b.Id == list.Room.Id);


                //$"Resesrvation {cont.ToString()}";

                context.PostAsync("Reservation " + cont.ToString() + "." + "Meeting Room: " + nameRoom +
                                 " Start Date: " + list.StartDate.ToShortDateString() +
                                 " Start Time: " + list.StartTime.ToShortTimeString() +
                                 " End Time: " + list.EndTime.ToShortTimeString() +
                                 " Attendant: " + list.Attendant.ToString() +
                                 "/n/r");
            }
        }
    }
}