using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarServices
{
    public interface IGoogleCalendarService
    {
        /// <summary>
        /// Meerdaagse events: [evenementNaam] SessieNaam
        /// 
        /// Bij aanmaken meeting/sessie -> aanmaken in google Calendar, in description de guid van het evenement meegeven waarvan het 
        /// deel uitmaakt. 
        /// </summary>


        #region Attendees
        //Uuid van attendee krijgen we vanuit DB, die moet niet gereturnd worden.Bij aanmaken vanuit kalendar wel, maar da's andere method.
        Task SignupForSessionAsync(string userGuid, string sessionGuid, EventAttendee attendee);
        Task LeaveSession(string userGuid, string sessionGuid, string attendeeGuid, string attendeeEmail);
        #endregion


        #region Sessions
        Task<List<Event>> GetAllUpcomingSessions(string calendarGuid, DateTime latestStartTime);
        Task<List<Event>> GetAllUpcomingSessionsUntilDate(string calendarGuid, DateTime latestStartTime);
        Task<List<Event>> GetAllUpcomingSessionsForEvent(string calendarGuid, string eventName);
        Task<List<Event>> GetAllSessionsForEvent(string calendarGuid, string eventName);
        //TODO: create session met alle mogelijke parameters (en die optioneel maken waar mogelijk)
        Task CreateSessionForEvent(string calendarGuid, string eventName, Event session);
        Task DeleteSession(string calendarGuid, string sessionGuid, string sessionName);
        Task<Event> UpdateSession(string calendarGuid, Event session, string eventName);
        #endregion

        #region Calendars
        Task CreateCalendar(string calendarName);
        Task DeleteCalendar(string calendarGuid);
        #endregion
        

    }
}
