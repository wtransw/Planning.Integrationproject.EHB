﻿using CalendarServices.Models.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.GData.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Calendar.v3.Data.Event;

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

        //static string ApplicationName = "Google Calendar Service for IntegrationProject";             //dubbelchecken in gCloud services.
        static string ApplicationName = "Google Calendar API Test";
        static string CalendarId = "planning.integrationproject.ehb@gmail.com";
        private UserCredential credential = null!;
        private CalendarService service = null!;
        ILogger Logger; 
        public ICalendarOptions CalendarOptions { get; set; }
        private OAuth2Parameters parameters = new OAuth2Parameters();

        public string CalendarGuid { get => CalendarId; set => CalendarId = value; }
        public GoogleCalendarService( )
        {
        }

        public async Task<Event> CreateAttendee(string eventGuid, string attendeeId, string email, string displayName, string? responseStatus, bool? optional = false)
        {
            var attendee = new EventAttendee()
            {
                Id = attendeeId,
                Email = email,
                DisplayName = displayName,
                ResponseStatus = responseStatus ?? "needsAction",
                Optional = optional,
                Organizer = false
            };
            var thisEvent = await GetSession(CalendarId, eventGuid);
            if (thisEvent != null)
            {
                thisEvent.Attendees = thisEvent.Attendees ?? new List<EventAttendee>();
                thisEvent.Attendees.Add(attendee);
                var updatedSession = await UpdateSession(CalendarId, thisEvent);
                return updatedSession;
            }
            return null;
        }
        public async Task<Event> CreateOrganizer(string eventGuid, string organizerId, string email, string displayName)
        {
            OrganizerData organizer = new OrganizerData()
            {
                Id = organizerId,
                DisplayName = displayName,
                Email = email,
                //Self = ????
            };

            var thisEvent = await GetSession(CalendarId, eventGuid);
            if (thisEvent != null)
            {
                thisEvent.Organizer = organizer;
                var updatedEvent = await service.Events.Update(thisEvent, CalendarId, eventGuid).ExecuteAsync();
                return updatedEvent;
            }
            return null;
        }

        public async Task<Calendar> CreateCalendar(string calendarName, string description, string? id)
        {
            var cal = new Calendar()
            {
                Summary = calendarName,
                Description = description,
                Location = "Desiderius",
                TimeZone = "Europe/Brussels"
            };
            if (id != null)
            {
                cal.Id = id;
            }
            return await service.Calendars.Insert(cal).ExecuteAsync();
        }

        public async Task<string> DeleteCalendar(string calendarGuid)
        {
            return await service.Calendars.Delete(calendarGuid).ExecuteAsync();
        }

        public async Task<Event> CreateSessionForEvent(string calendarGuid, string eventName, Event session)
        {
            session.Summary = $"[{eventName}] {session.Summary}";
            return await service.Events.Insert(session, calendarGuid).ExecuteAsync();

            // Alternatieve methode:
            //EventsResource.InsertRequest req = new EventsResource.InsertRequest(service, session, calendarGuid);
            //return await req.ExecuteAsync();
        }

        public async Task<string> DeleteSession(string calendarGuid, string? sessionGuid, string? sessionName)
        {
            sessionGuid = sessionGuid ?? await GetEventId(calendarGuid, sessionName);
            if (sessionGuid != null)
            {
                return await service.Events.Delete(calendarGuid, sessionGuid).ExecuteAsync();
            }
            else return "Session id or name needed";
        }

        public async Task<List<Event>> GetAllSessionsForEvent(string calendarGuid, string eventName)
        {
            var allSessions = await service.Events.List(calendarGuid).ExecuteAsync();
            return allSessions.Items.Where(x => x.Summary.Contains($"[{eventName}]")).ToList();
        }

        public async Task<List<Event>> GetAllUpcomingSessions(string calendarGuid)
        {
            var allSessions = await service.Events.List(calendarGuid).ExecuteAsync();
            return allSessions.Items.ToList();  //sws deze in de toekomst of hier nog een filter op toevoegen?
        }

        public async Task<List<Event>> GetAllUpcomingSessionsUntilDate(string calendarGuid, DateTime latestStartTime)
        {
            var allSessions = await service.Events.List(calendarGuid).ExecuteAsync();
            return allSessions.Items.Where(x => x.Start.DateTime < latestStartTime).ToList();
        }

        public async Task<Event> RemoveAttendeeFromSession(string userGuid, string sessionGuid, string attendeeGuid, string attendeeEmail)
        {
            var session = await GetSession(CalendarId, sessionGuid);
            session.Attendees = session.Attendees.Where(x => x.Id != attendeeGuid && x.Email != attendeeEmail).ToList();
            return await UpdateSession(CalendarId, session);
        }

        // get attendeeby email
        //public async Task<EventAttendee> GetAttendeeByEmail(string userGuid, string sessionGuid, string attendeeGuid, string attendeeEmail)
        public async Task <EventAttendee> GetAttendeeByEmail(string sessionGuid, string attendeeEmail)
        {
           // var allSessions = await service.Events.List(CalendarId).ExecuteAsync();
          //  var sessionsWithThisAttendee = allSessions.Items.Where(x => x.Attendees.Any(y => y.Email== attendeeEmail));
            //return allSessions;
            var session=await GetSession(CalendarId, sessionGuid);
            //var attendee = session.Attendees.FirstOrDefault(x => x.Email == attendeeEmail);
            var attendee = session.Attendees.FirstOrDefault(x => x.Email.StartsWith("b"));
            return attendee;
        }


        public async Task<Event> AddAttendeeToSessionAsync(string sessionGuid, EventAttendee attendee)
        {
            var session = await GetSession(CalendarId, sessionGuid);
            session.Attendees = session.Attendees ?? new List<EventAttendee>();
            session.Attendees.Add(attendee);
            return await UpdateSession(CalendarId, session);
        }

        public async Task<EventAttendee> UpdateAttendee(EventAttendee attendee)
        {
            var allSessions = await service.Events.List(CalendarId).ExecuteAsync();
            var sessionsWithThisAttendee = allSessions.Items.Where(x => x.Attendees.Any(y => y.Id == attendee.Id));
            foreach (var session in sessionsWithThisAttendee)
            {
                session.Attendees = session.Attendees.Where(x => x.Id != attendee.Id).ToList();
                session.Attendees.Add(attendee);
                await UpdateSession(CalendarId, session);
            }
            return attendee;
        }

        public async Task<Event> UpdateSession(string calendarGuid, Event session)
        {
            return await service.Events.Update(session, calendarGuid, session.Id).ExecuteAsync();
        }

        public async Task<Event> GetSession(string calendarId, string eventId)
        {
            if (!string.IsNullOrEmpty(calendarId))
                return await service.Events.Get(calendarId, eventId).ExecuteAsync();

            else 
                return await service.Events.Get(CalendarId, eventId).ExecuteAsync();
        }

        public Channel CreateChannel(string address, string? id, int? ttlMinutes)
        {
            var channel = new Channel()
            {
                Id = id ?? new Guid().ToString(),
                Address = address,
                Type = "web_hook"
            };
            channel.Payload = true;
            
            if (ttlMinutes != null)     //if not specified, standard value is 7 days.
            {
                var endTimeChannel = DateTime.Now.AddMinutes((int)ttlMinutes)
                                            .ToUniversalTime()
                                            .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                                            .TotalMilliseconds;
                channel.Expiration = (long)endTimeChannel;
            }
            
            return channel;
        }
        public Task<Stream?> WatchEvents()
        {
            /*
                https://developers.google.com/calendar/api/guides/push
                Note that the Google Calendar API will be able to send notifications to this HTTPS address only if there is
                a valid SSL certificate installed on your web server. 
                Invalid certificates include: Self-signed certificates.
            */
            throw new NotImplementedException();
        }
        private async Task<string?> GetEventId(string calendarId, string? sessionName)
        {
            if (sessionName != null)
                return (await service.Events.List(calendarId).ExecuteAsync()).Items.FirstOrDefault(e => e.Summary == sessionName)?.Id;
            else return null;
        }

        public Task CreateCalendarService(ICalendarOptions calendarOptions)
        {
            this.CalendarOptions = calendarOptions;
            //CreateCredential();
            GetCredential();

            // Create Google Calendar API service with the credential.
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            return Task.CompletedTask;
        }

        private void GetCredential()
        {
            try
            {
                parameters = GetParameters();
                Google.GData.Client.OAuthUtil.RefreshAccessToken(parameters);
                var flow = GetFlow();
                var token = GetToken(); 

                credential = new UserCredential(flow, Environment.UserName, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting credential: " + ex);
                //Console.WriteLine(ex + " Retry with v3 now.");
                //retryWithV3();
            }
        }

        private void retryWithV3()
        {
            try
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

                    parameters.AccessToken = credential.Token.AccessToken;
                    parameters.RefreshToken = credential.Token.RefreshToken;    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Retry with v3 failed: " + ex);
            }
        }

        private TokenResponse GetToken() => new TokenResponse
        {
            AccessToken = parameters.AccessToken,
            RefreshToken = parameters.RefreshToken
        };

        private GoogleAuthorizationCodeFlow GetFlow() => new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = parameters.ClientId,
                ClientSecret = parameters.ClientSecret,
            },
            Scopes = Scopes,
            DataStore = new FileDataStore("Store")
        });

        private Google.GData.Client.OAuth2Parameters GetParameters() => new OAuth2Parameters()
        {
            ClientId = CalendarOptions.ClientId,
            ClientSecret = CalendarOptions.ClientSecret,
            AccessToken = CalendarOptions.AccessToken,
            RedirectUri = CalendarOptions.RedirectUri,
            RefreshToken = CalendarOptions.RefreshToken,
            AccessType =   CalendarOptions.AccessType,
            TokenType =    CalendarOptions.TokenType,
            Scope = CalendarOptions.Scope,
            //Scope = "http://www.google.com/calendar/feeds/default/",
            //TokenExpiry = DateTime.Parse("2022-05-09T19:19:55.758Z").AddSeconds(3599)
            AuthUri = "https://accounts.google.com/o/oauth2/auth",
            TokenUri = "https://oauth2.googleapis.com/token",
            TokenExpiry = DateTime.Parse("2022-05-09T21:19:55.758+02:00").AddSeconds(3599),
            ResponseType = "code",

        };

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
