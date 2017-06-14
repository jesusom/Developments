using BotServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Outlook;


namespace BotServices.Utilities
{
    public static class Outlook
    {
        public static void DeleteEventOutlook (CancelMeetingRoomForm data)
        {
            try
            {
                Application app = new Application();

                MAPIFolder calendar = app.Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                Items calendarItems = calendar.Items;
                AppointmentItem item = calendarItems["Test Appointment"] as AppointmentItem;
                RecurrencePattern pattern = item.GetRecurrencePattern();
                AppointmentItem itemDelete = pattern.GetOccurrence(new DateTime(2006, 6, 28, 8, 0, 0));

                if (itemDelete != null)
                {
                    itemDelete.Delete();
                }
            }
            catch (System.Exception)
            {

                
            }

        }

        public static void CreateEventOutlook(BookMeetingRoomForm data)
        {
            try
            {
                Application app = null;
                app = new Application();

                AppointmentItem appt = app.CreateItem(OlItemType.olAppointmentItem) as AppointmentItem;
                appt.Subject = "Notice: book a meeting room";
                appt.AllDayEvent = false;
                appt.Start = DateTime.Parse(data.StartDate.ToShortDateString() + " " + data.StartTime);
                //appt.End = DateTime.Parse(data.FinalDate.ToShortDateString() + " " + data.FinalTime);
                appt.Importance = OlImportance.olImportanceHigh;
                appt.Location = "Sala Pozuelo";
                appt.Body = "Room type: " + data.TypeRoom.Value.ToString() + "Number of people: + " + data.Attendant.ToString();
                appt.Display(false);
            }
            catch (System.Exception ex)
            {
               
            }

        }
    }
}