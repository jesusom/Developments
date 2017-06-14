using BotServices.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BotServices.Models
{
    [Serializable]
    public class CancelMeetingRoomForm
    {
        [Prompt("What day are we looking for?: {||}", AllowDefault = BoolDefault.True)]
        [Describe("Date of the Reservation ")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date (ex: 2/14/2017)")]
        [Required]
        public DateTime StartDate { get; set; }
        [Prompt("What is the meeting room?: {||}", AllowDefault = BoolDefault.True)]
        [Required]
        [Describe("Name of the meeting room ")]
        public string NameRoom { get; set; }

        public static IForm<CancelMeetingRoomForm> BuildForm()
        {
            //var result = false;

            //OnCompletionAsyncDelegate<CancelMeetingRoomForm> processOrder = async (context, state) =>
            //{
            //    //result = SaveChangesfromCancelBooking(context, state);

            //    if (result)
            //    {
            //        await context.PostAsync("Great....Cancelled Reservation to the meeting room: " + state.NameRoom);
            //        await context.PostAsync("Thanks for using our service");
            //        await context.PostAsync("Bye!! ;-)");
            //    }
            //    else
            //    {
            //        await context.PostAsync("Reservation not found. Try it again, please!!!!!!");
            //    }

            //    //context.Done<object>(null);
            //};

            return new FormBuilder<CancelMeetingRoomForm>()
                //.Message("Let´s go to cancel a meeting room")
                .Field(nameof(StartDate))
                .Field(nameof(NameRoom))
                //.OnCompletion(processOrder)
                .Build();
        }

        //private static bool SaveChangesfromCancelBooking(IDialogContext context, CancelMeetingRoomForm entity)
        //{
        //    var db = new Model.EntityModelContainer();
        //    var user = context.Activity.From.Name;

        //    var itemToRemove = db.Bookings.SingleOrDefault(b => b.Room.NameRoom == entity.NameRoom
        //                                            && b.EmplyeeId == user
        //                                            && b.StartDate == entity.StartDate);
        //    db.Bookings.Remove(itemToRemove);
        //    db.SaveChanges();

        //    return true;
        //}
    }
}