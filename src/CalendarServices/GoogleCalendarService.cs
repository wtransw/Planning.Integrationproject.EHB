using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalendarServices
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        //Na aapassen van de scopes niet vergeten je token.json file te deleten in je build direcory!!!!!!!!!!!
        //Scope aanpassen indien nodig ivm veiligheid. Staat nu open.
        static string[] Scopes = {
            CalendarService.Scope.Calendar,
            CalendarService.Scope.CalendarEvents,
            CalendarService.Scope.CalendarSettingsReadonly
        };

        static string ApplicationName = "Google Calendar Service for IntegrationProject";             //dubbelchecken in gCloud services.
        static string CalendarId = "planning.integrationproject.ehb@gmail.com"; //TODO: uit AppSettings halen.
        //static string eventIdMeeting120322 = "0pmjplojtl1hsp897u8s2shns4";      
        static string eventIdMeeting120322 = "aa5uugl3gh8hsmq491a373p87o";
        private UserCredential credential;
        private CalendarService service;

        public GoogleCalendarService()
        {
            CreateCalendarService();
        }

        public Task CreateCalendar(string calendarName)
        {
            throw new NotImplementedException();
        }

        public Task CreateSessionForEvent(string calendarGuid, string eventName, Event session)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCalendar(string calendarGuid)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSession(string calendarGuid, string sessionGuid, string sessionName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetAllSessionsForEvent(string calendarGuid, string eventName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetAllUpcomingSessions(string calendarGuid, DateTime latestStartTime)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetAllUpcomingSessionsForEvent(string calendarGuid, string eventName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetAllUpcomingSessionsUntilDate(string calendarGuid, DateTime latestStartTime)
        {
            throw new NotImplementedException();
        }

        public Task LeaveSession(string userGuid, string sessionGuid, string attendeeGuid, string attendeeEmail)
        {
            throw new NotImplementedException();
        }

        public Task SignupForSessionAsync(string userGuid, string sessionGuid, EventAttendee attendee)
        {
            throw new NotImplementedException();
        }

        public Task<Event> UpdateSession(string calendarGuid, Event session, string eventName)
        {
            throw new NotImplementedException();
        }


        private void CreateCalendarService()
        {
            CreateCredential();
            
            // Create Google Calendar API service with the credential.
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
        private void CreateCredential()
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
        }
    }
}
