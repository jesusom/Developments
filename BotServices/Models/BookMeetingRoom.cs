using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BotServices.Models
{
    public enum TypeMeetingRoom{
        with_projector,
        without_projector
    }
    [Serializable]
    public class BookMeetingRoomForm
    {
        const string With_Projector = "with Projector";
        //const string Without_Projector = "without Projector";

        [Prompt("What day do you want it? (Format english.ex: 02/14/2017): {||}")]
        [Describe("Date of the Reservation ")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date (ex: 02/14/2017)")]
        [Required]
        public DateTime StartDate {get; set;}
        //[Prompt("End date of reservation (ex: 02/14/2017): {||}", AllowDefault = BoolDefault.True)]
        //[Describe("Day the reservation ends  ")]
        //[DataType(DataType.DateTime, ErrorMessage = "Please enter a valid date (ex: 02/14/2017)")]
        //public DateTime FinalDate {get; set;}
        [Prompt("What time you want it? (ex: 9:00): {||}")]
        [Describe("Time of the reservation  ")]
        [DataType(DataType.Time, ErrorMessage = "Please enter a valid time (ex: 13:00)")]
        public DateTime StartTime {get;set;}
        [Prompt("Until what time do you want it? (ex: 17:00): {||}", AllowDefault = BoolDefault.True)]
        [Describe("Time the reservation ends ")]
        [DataType(DataType.Time, ErrorMessage = "Please enter a valid time (ex: 13:00)")]
        public DateTime EndTime { get; set; }
        [Prompt("How many people? {||}", AllowDefault = BoolDefault.True)]
        [Describe("Number of people who will be in the meeting room ")]
        [Numeric(1,500)]
        public int Attendant { get; set; }
        [Prompt("Room type: {||}", AllowDefault = BoolDefault.True)]
        public TypeMeetingRoom? TypeRoom { get; set; }


        public static IForm<BookMeetingRoomForm> BuildForm()
        {
            var nameRoom = string.Empty;

            OnCompletionAsyncDelegate<BookMeetingRoomForm> processOrder = async (context, state) =>
            {
                SaveChangesfromBooking(context.Activity.From.Name, state, out nameRoom);

                await context.PostAsync("You did it....!!!!!");
                await context.PostAsync("You have the meeting Room: " + nameRoom );

                context.Done<object>(null);
            };

            return new FormBuilder<BookMeetingRoomForm>()
                .Message("Let´s go to book a meeting room")
                .Field(nameof(StartDate))
                .Field(nameof(StartTime))
                .Field(nameof(EndTime))
                .Field(nameof(Attendant))
                .Field(nameof(TypeRoom))
                .OnCompletion(processOrder)
                .AddRemainingFields()
                .Build();
        }

        private static void SaveChangesfromBooking(string userName, BookMeetingRoomForm entity, out string nameRoom)
        {
            var db = new Model.EntityModelContainer();
            var typeRoom = false;
            nameRoom = string.Empty;

            if (entity.TypeRoom.Value.ToString() == With_Projector)
            {
                typeRoom = true;
            }

            var listNomRoom = db.RoomSet.Where(b => b.Projector == typeRoom && b.Size >= entity.Attendant);

            db.Bookings.Add(new Model.Booking
            {
                EmplyeeId = userName,
                StartDate = entity.StartDate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Attendant = entity.Attendant,
                Room = listNomRoom.First(),
                IsCancel = false,
            });

            nameRoom = listNomRoom.First().NameRoom.ToString();

            db.SaveChanges();
        }
    }
}