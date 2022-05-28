using CalendarServices.Models.Configuration;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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
        string CalendarGuid { get; set; }
        ICalendarOptions CalendarOptions { get; set; }
        Task CreateCalendarService(ICalendarOptions calendarOptions);

        #region Attendees
        //Hier het updated event telkens returnen? 
        Task<Event> CreateAttendee(string eventGuid, string attendeeId, string email, string displayName, string? responseStatus, bool? optional);
        Task<Event> CreateOrganizer(string eventGuid, string organizerId, string email, string displayName);
        //Uuid van attendee krijgen we vanuit DB, die moet niet gereturnd worden.Bij aanmaken vanuit kalendar wel, maar da's andere method.
        Task<EventAttendee> UpdateAttendee(EventAttendee attendee);
        Task<Event> AddAttendeeToSessionAsync(string sessionGuid, EventAttendee attendee);
        Task<Event> RemoveAttendeeFromSession(string userGuid, string sessionGuid, string attendeeGuid, string attendeeEmail);
        Task<EventAttendee?> GetAttendeeByEmail(string attendeeEmail);
        Task<EventAttendee?> GetAttendeeByUuid(string uuid);
        #endregion

        #region Sessions
        Task<List<Event>> GetAllUpcomingSessions(string calendarGuid);
        Task<List<Event>> GetAllUpcomingSessionsUntilDate(string calendarGuid, DateTime latestStartTime);
        Task<List<Event>> GetAllSessionsForEvent(string calendarGuid, string eventName);
        //TODO: create session met alle mogelijke parameters (en die optioneel maken waar mogelijk)
        Task<Event> GetSession(string calendarGuid, string eventId);
        Task<Event> CreateSessionForEvent(string calendarGuid, string eventName, Event session);
        Task<string> DeleteSession(string calendarGuid, string? sessionGuid, string? sessionName);
        Task<Event> UpdateSession(string calendarGuid, Event session);
        #endregion

        #region Calendars
        Task<Calendar> CreateCalendar(string calendarName, string description, string? id);
        Task<string> DeleteCalendar(string calendarGuid);
        #endregion

        #region Callbacks
        Channel CreateChannel(string address, string? id, int? ttlMinutes);
        Task<Stream?> WatchEvents();
        #endregion

    }
}
