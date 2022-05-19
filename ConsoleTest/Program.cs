using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using CalendarServices.Models;
using CalendarServices;
using Google.GData.Client;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using CalendarServices.Models.Configuration;

class Program
{
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/calendar-dotnet-quickstart.json

    //Scope aanpassen indien nodig ivm veiligheid. Staat nu open.
    //Na aapassen van de scopes niet vergeten je token.json file te deleten in je build direcory!!!!!!!!!!! !!!!!!!!!!
    static string[] Scopes = {
        CalendarService.Scope.Calendar,
        CalendarService.Scope.CalendarEvents,
        CalendarService.Scope.CalendarSettingsReadonly
    };
    static CalendarService service;
    static string ApplicationName = "Google Calendar API Test";             //dubbelchecken in gCloud services.
    static string CalendarId = "planning.integrationproject.ehb@gmail.com"; //TODO: uit AppSettings halen.
    //static string eventIdMeeting120322 = "0pmjplojtl1hsp897u8s2shns4";      
    static string eventIdMeeting120322 = "aa5uugl3gh8hsmq491a373p87o";


    static void Main(string[] args)
    {
        //Flow
        /*
         *          * andere deeltjes *
         *              rabbitmq
         *               * wij *
         *        igoogleCalendarService
         *            google Calendar
         */

        ////var brol = testAttendeeXml();
        //try
        //{
        //    var brol = ObjectToXmlTest().GetAwaiter().GetResult();
        //    Console.WriteLine(brol);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //}
        //;
        //return;

        UserCredential credential = default;

        //using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        //{
        //    // The file token.json stores the user's access and refresh tokens, and is created
        //    // automatically when the authorization flow completes for the first time.
        //    string credPath = "token.json";
        //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        GoogleClientSecrets.FromStream(stream).Secrets,
        //        Scopes,
        //        "user",
        //        CancellationToken.None,
        //        new FileDataStore(credPath, true)).Result;
        //    Console.WriteLine("Credential file saved to: " + credPath);
        //}

        

        /*	"access_token": "ya29.A0ARrdaM-yXDGRhX1odkuwY7QyptjBTFQA-9H1J335DYH-LfmFGvwCKLjNhz6VXCqRIJ6vukNU1eD1omzpVoFNLZ9ScdNe4BAuwzOu9cmHsCgkhSLKKv8R03t6ASSFNDgWMN2anpBQUrwHxN1UbDtOfHTygiB9",
	"token_type": "Bearer",
	"expires_in": 3599,
	"refresh_token": "1//03Ih4E5jljabiCgYIARAAGAMSNwF-L9IrATlwBwkvY3YOx5UbNR7LKlVpvOeLje6guma2BYwCNt45C3giViPCYNzr62-DTwAkTNk",
	"scope": "https://www.googleapis.com/auth/calendar.events https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.settings.readonly",
	"Issued": "2022-05-06T18:01:27.142+02:00",
	"IssuedUtc": "2022-05-06T16:01:27.142Z"
         */

        //Google.GData.Client.RequestSettings settings = new RequestSettings("integrationprojplanningwt2022");



        /*
         * 
         *     "client_id": "281496544249-7l0127vpa5kuetv6r4a10b13g5hd8jia.apps.googleusercontent.com",
    "project_id": "integrationprojplanningwt2022",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "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",
    "redirect_uris": [ "urn:ietf:wg:oauth:2.0:oob", "http://localhost" ]
         * 
         */
        Google.GData.Client.OAuth2Parameters parameters = new OAuth2Parameters()
        {
            //
            ClientId = "281496544249-7l0127vpa5kuetv6r4a10b13g5hd8jia.apps.googleusercontent.com",
            ClientSecret = "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",
            //AccessToken = "ya29.A0ARrdaM-yXDGRhX1odkuwY7QyptjBTFQA-9H1J335DYH-LfmFGvwCKLjNhz6VXCqRIJ6vukNU1eD1omzpVoFNLZ9ScdNe4BAuwzOu9cmHsCgkhSLKKv8R03t6ASSFNDgWMN2anpBQUrwHxN1UbDtOfHTygiB9", 
            RedirectUri = "urn:ietf:wg:oauth:2.0:oob",
            //RefreshToken = "1//03Ih4E5jljabiCgYIARAAGAMSNwF-L9IrATlwBwkvY3YOx5UbNR7LKlVpvOeLje6guma2BYwCNt45C3giViPCYNzr62-DTwAkTNk",
            AccessToken = "ya29.A0ARrdaM_LVBKkYZQ5GThtoEd7JsgyN65cTDgaJpZ_DTY0s6NVWstRbdOErPGKRxq-A0BJVQpRrNlGztE9WRtXbBl27fEENHvSwRXbwO-Wdz40U8kpyhUejKctx1-yBX4I8xW3qJEmbt5hBoX3AZuJDed0S2fTxw",
            RefreshToken = "1//099dYhxDE0AE4CgYIARAAGAkSNwF-L9Ir5h8jYPv_sKWIX7ERmLOGax1ie6id05E14mCUc7jO9NQOtRMf6r3u30tFAv7PMhbPsfE",
            AccessType = "offline",
            TokenType = "refresh",
            Scope = "https://www.googleapis.com/auth/calendar.events https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.settings.readonly",
             //ApprovalPrompt = "none",
              AuthUri = "https://accounts.google.com/o/oauth2/auth",
               TokenUri = "https://oauth2.googleapis.com/token",
                TokenExpiry = DateTime.Parse("2022-05-09T21:19:55.758+02:00").AddSeconds(3599),
                 ResponseType = "code",
                  

        };
        try
        {
            ;
            Google.GData.Client.OAuthUtil.RefreshAccessToken(parameters);
            ;

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = parameters.ClientId,
                    ClientSecret = parameters.ClientSecret,
                },
                Scopes = Scopes,
                DataStore = new FileDataStore("Store")
            });

            var token = new TokenResponse
            {
                AccessToken = parameters.AccessToken,
                RefreshToken = parameters.RefreshToken
            };

            credential = new UserCredential(flow, Environment.UserName, token);


            //using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            //{
            //    // The file token.json stores the user's access and refresh tokens, and is created
            //    // automatically when the authorization flow completes for the first time.
            //    string credPath = "token.json";

            //    var googleClientSecrets = GoogleClientSecrets.FromFile("credentials.json");
            //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(googleClientSecrets.Secrets, Scopes, "user", CancellationToken.None).GetAwaiter().GetResult();

            //Thread.Sleep(3000);
            //}

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ;
        }




        // Create Google Calendar API service with the credential.
        service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var newEvent = new Event()
        {
            Summary = "Kersttrip 2022",
            Location = "Koln",
            Description = "Shoppen met gluhwein",
            Start = new EventDateTime()
            {
                DateTime = DateTime.Parse("2022-12-19T08:00:00-07:00"),
                TimeZone = "Europe/Zurich"
            },
            End = new EventDateTime()
            {
                DateTime = DateTime.Parse("2022-12-22T17:00:00-07:00"),
                TimeZone = "Europe/Zurich"
            },
            //Recurrence = new string[] {
            //    "RRULE:FREQ=DAILY;COUNT=2"
            //},
            Attendees = new EventAttendee[] {
                new EventAttendee
                {
                    //Id = Id uit de CRM?????
                    Email = "wouter.anseeuw@student.ehb.be",
                    DisplayName = "Wouter Anseeuw",
                    ResponseStatus = "accepted",
                     
                    Organizer = true                //bij ons de spreker
                },
                new EventAttendee
                {
                    Email = "BillyJoel@music.com",
                    DisplayName = "Billy Joel",
                    ResponseStatus = "accepted",
                    Organizer = false               //bij ons dus een gast
                    //resource = ????
                }
            }
        };


        try
        {

            /*
            //EventsResource.InsertRequest request = new EventsResource.InsertRequest(service, newEvent, CalendarId);
            //Event response = request.Execute();

            //service.Events.Watch()


            //watch google calendar for changes
            var channel = new Channel();
            
            //Kestrel is ingesteld op poort 5000 voor http, 7001 voor https.
            channel.Address = "https://0.0.0.0:7001/";  

            //webhook.site gebruiken als webhook om https callbacks te testen
            //channel.Address = "https://webhook.site/c9d0cbe8-5f84-4fde-9647-0bf95cf14c96";
            channel.Id = "5";
            channel.Type = "web_hook";
            channel.Payload = true;

            //datetime to unix timestamp
            var endTimeChannel = DateTime.Now.AddMinutes(10).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            channel.Expiration = (long)endTimeChannel;        //verloopt binnen een half uur.
                                                              //channel.Type = "stream";

            var stream = service.Events.Watch(channel, CalendarId).ExecuteAsStream();

            //using (var reader = new StreamReader(stream))
            //{
            //    bool watching = true;
            //    while (watching)
            //    {
            //        while (reader.Peek() >= 0)
            //        {
            //            var line = reader.ReadLine();
            //            Console.WriteLine(line);
            //        }
            //        Thread.Sleep(1000);
            //    }
            //}

            var _ = ShowEvents(service).ConfigureAwait(false);
            Thread.Sleep(10000);
            */

            var elfMeiEvent = new Event
            {
                Summary = "Elf Mei",
                Location = "Verrassing",
                Description = "Wortelen planten",
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2022-05-11T08:00:00-07:00"),
                    TimeZone = "Europe/Zurich"
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2022-05-11T17:00:00-07:00"),
                    TimeZone = "Europe/Zurich"
                }
            };
            /*
            EventsResource.InsertRequest request = new EventsResource.InsertRequest(service, elfMeiEvent, CalendarId);
            Event response = request.Execute();

            Thread.Sleep(5000);

            var _ = ShowEvents(service).ConfigureAwait(false);
            
            ;


            Thread.Sleep(10000);
            */
         testAttendeeXml();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            ;
        }



        //Add an Attendee
        EventAttendee newAttendee = new()
        {
            Email = "NieuweGenodigde123456@gmail.com",
            DisplayName = "Nieuwe Genodigde",
            Comment = "Welkom"
        };

        //AddAttendee(service, eventIdMeeting120322, newAttendee).ConfigureAwait(false);

        //Thread.Sleep(5000);
        //Toon overzicht op scherm.
        //ShowEvents(service).ConfigureAwait(false);
       

    }//--------- end static main

    async static Task<Event?> GetEvent(CalendarService service, string eventId)
    {
        EventsResource.ListRequest request = service.Events.List(CalendarId);
        var req = await request.ExecuteAsync();

        return req.Items.FirstOrDefault(ev => ev.Id == eventId);
    }

    async static Task ShowEvents(CalendarService service)
    {

        // Define parameters of request.
        EventsResource.ListRequest request = service.Events.List("primary");
        //EventsResource.ListRequest request = service.Events.List(CalendarId);
        request.TimeMin = DateTime.Now;
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.MaxResults = 10;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        // List events.
        Events events = request.Execute();
        Console.WriteLine("Upcoming events:");
        if (events.Items != null && events.Items.Count > 0)
        {
            foreach (var eventItem in events.Items)
            {
                Console.WriteLine($"{Environment.NewLine} -= EVENT =-");
                string when = eventItem.Start.DateTime.ToString();
                if (String.IsNullOrEmpty(when))
                {
                    when = eventItem.Start.Date;
                }
                Console.WriteLine($"{eventItem.Id}: {eventItem.Summary} - {when}");
                //Console.WriteLine($"{eventItem.Summary} {when}");
                Console.WriteLine($"Organisator: {eventItem.Organizer}");
                Console.WriteLine($"Locatie: {eventItem.Location}");
                var numberOfAttendees = eventItem.Attendees?.Count ?? 0;
                Console.WriteLine($"Aantal aanwezigen: {numberOfAttendees}");
                if (numberOfAttendees > 0)
                    foreach (var attendee in eventItem.Attendees!)
                    {
                        Console.WriteLine($"{attendee.Id}: {attendee.DisplayName} ({attendee.Email}) - Antwoord: {attendee.ResponseStatus}");
                    }
            }
        }
        else
        {
            Console.WriteLine("No upcoming events found.");
        }


    }

    async static Task AddAttendee(CalendarService service, string eventId, EventAttendee eventAttendee)
    {
        var eventItem = await GetEvent(service, eventId);
        if (eventItem is not null && eventItem.Start.DateTime > DateTime.UtcNow)
        {
            eventItem.Attendees = eventItem.Attendees ?? new List<EventAttendee>();
            eventItem.Attendees.Add(eventAttendee);
            try
            {
                var aantalGenodigden = eventItem.Attendees?.Count();
                var updatedEvent = await service.Events.Update(eventItem, CalendarId, eventIdMeeting120322).ExecuteAsync();
                var aantalGenodigdenNaInsert = updatedEvent.Attendees?.Count();

                Console.WriteLine($"aantal genodigden: {aantalGenodigden.ToString() ?? "unavailable"}");

                bool isGelukt = eventItem.Attendees.First(a => a.Email == eventAttendee.Email) is not null && aantalGenodigdenNaInsert == aantalGenodigden + 1;

                var lijstMetGenodigden = updatedEvent.Attendees!.ToList();
                ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else
            Console.WriteLine($"Kon geen event in de toekomst vinden met Id {eventId}");
    }

    
    static async void testAttendeeXml()
    {
        

        var xmlString = File.ReadAllText(@"C:\Users\woute\Source\Repos\Planning.Integrationproject.EHB.Forked\src\PlanningApi\XmlSchemas_old\AttendeeEvent_1.xml");
        
        //var xmlPath = @"C:\Users\Jan Met Pet\Source\Repos\Planning.Integrationproject.EHB\src\PlanningApi\XmlSchemas_old\AttendeeEvent_1.xml";

        //  var xmlString = File.ReadAllText(@"c:\temp\brol.xml");
        // return DeSerializeXml(xmlString);

        //var xmlString = File.ReadAllText(xmlPath);
        //DeSerializeXml(xmlString);
        Console.WriteLine(SourceEnum.PLANNING.ToString());

        //DeSerializeXml(xmlString);

        

        var objectje = await ObjectToXmlTest();


        if (!objectje.StartsWith("Error"))
            DeSerializeXml(objectje);
        else
            Console.WriteLine($"Object was null, kon niet afhandelen: {objectje}.");

    }

    //void 
    static void DeSerializeXml(string xmlString)
    {
        XmlRootAttribute xRoot = new XmlRootAttribute();
        xRoot.ElementName = "AttendeeEvent";
        //xRoot.ElementName = "SessionAttendeeEvent";
        //xRoot.ElementName = "SessionEvent";

        //xRoot.Namespace = "http://www.brol.com";
        xRoot.IsNullable = true;

        var xmlSerializer = new XmlSerializer(typeof(PlanningAttendee), xRoot);
        //var xmlSerializer = new XmlSerializer(typeof(PlanningSession), xRoot);
        //var xmlSerializer = new XmlSerializer(typeof(PlanningSessionAttendee), xRoot);

        using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

        var mijnModel = (PlanningAttendee)xmlSerializer.Deserialize(reader);
        //var mijnModel = (PlanningSessionAttendee)xmlSerializer.Deserialize(reader);
        //var mijnModel = (PlanningSession)xmlSerializer.Deserialize(reader);

        ;

        //HandleAttendee(mijnModel);

        //return attendee is not null ? (attendee.Name ?? "Doe" + attendee.LastName ?? "John") : "";
    }

    static async void HandleAttendee(PlanningAttendee attendee)
    {
        try
        {
            Console.WriteLine(Environment.NewLine + "Getting attendee with email " + attendee.Email);
            var gcal = new GoogleCalendarService(service);

            var calOptions = new CalendarOptions()
            {
                CalendarGuid = "planning.integrationproject.ehb@gmail.com",
                ClientId = "281496544249-7l0127vpa5kuetv6r4a10b13g5hd8jia.apps.googleusercontent.com",
                ClientSecret = "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",
                AccessToken = "ya29.A0ARrdaM88oOwgA7BctBiK6gH5a0ZpH6IgoQe5JBcFV6l1GaZjflkY5q1BPMstQnqSDlL0cXpyl-J9sHaNWwik8Bs4-ha3mdPveR1l-FJTObvvGfe-xzVkX1-VgrPbhbY3sr_CyOS1T9DQ4vgfIRlxX_QXfvNG",
                RedirectUri = "urn:ietf:wg:oauth:2.0:oob",
                RefreshToken = "1//033VPyDOCwwmGCgYIARAAGAMSNwF-L9IrXqh9ANuGlCrQBTPiVejDId4Gx-r0gHySPRmB8C9gpnNtDhEaLIAYw0AZmDi7bnacUHc",
                AccessType = "offline",
                TokenType = "refresh",
                Scope = "https://www.googleapis.com/auth/calendar.events https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.settings.readonly"
            };
            await gcal.CreateCalendarService(calOptions);

            var allSessions = await gcal.GetAllUpcomingSessions(gcal.CalendarGuid);

            var firstSession = allSessions.FirstOrDefault();
            var legeAttendee = new EventAttendee() { Email = "Rogekepateeke@rogeke.com", DisplayName = "Geen idee wat mijn naam is." };

            if (firstSession != null)
                await gcal.AddAttendeeToSessionAsync(firstSession.Id, legeAttendee);


            ;



            var attendeeUitGoogleCalendar = await gcal.GetAttendeeByEmail(attendee.Email);

            //updaten wat we moeten updaten, bijvoorbeeld zijn naam
            if (attendeeUitGoogleCalendar is not null)
            {
                attendeeUitGoogleCalendar.DisplayName = attendee.LastName + attendee.Name;
                //attendeeUitGoogleCalendar.Comment = attendee.VatNumber;

                await gcal.UpdateAttendee(attendeeUitGoogleCalendar);
            }


        }
        catch (Exception ex)    
        { 
            Console.WriteLine(ex.Message); 
        }



       // Console.WriteLine(gcal.GetAttendeeByEmail("aa5uugl3gh8hsmq491a373p87o", "nieuwegenodigde123456@gmail.com"));
       // Console.WriteLine(gcal);
    }
   





    static async Task<string> ObjectToXmlTest()
    {
        var logger = new NLog.LogFactory().GetCurrentClassLogger();
        var pS = new PlanningService(logger);

        object obj;
        string testUuid = "12345678901234567890123456789012";
        //var attendee = new PlanningAttendee { Name = "Wouter", LastName = "A", Email = "my@mail.here", VatNumber = "", Version = 12 };
        //var attendee = new PlanningAttendee { Name = "Wouter", LastName = "A", Email = "my@mail.here", EntityVersion = "12" };


        var attendee = new PlanningAttendee()
        {
            Email = "no@email.yet",
            EntityVersion = 1,
            Name = "Jean",
            LastName = "Avec la casquette",
            Method = MethodEnum.CREATE,
            Source = SourceEnum.PLANNING,
            SourceEntityId = "no@email.yet",
            UUID_Nr = Guid.NewGuid().ToString(),
            EntityType = "Attendee"
        };
        var session = new PlanningSession()
        {
            Title = "eerste sessie",
            StartDateUTC = new DateTime(2022, 12, 01),
            EndDateUTC = new DateTime(2022, 12, 02),
            EntityType = "SessionEvent",
            IsActive = true,
            EntityVersion = 2,
            Method = MethodEnum.UPDATE,
            OrganiserUUID = testUuid,
            SourceEntityId = "ikgebruikeenemail@als.id",
            Source = SourceEnum.PLANNING,
            UUID_Nr = Guid.NewGuid().ToString()
        };
        var sessionAttendee = new PlanningSessionAttendee()
        {
            UUID_Nr = Guid.NewGuid().ToString(),
            EntityVersion = 1,
            Method = MethodEnum.CREATE,
            Source = SourceEnum.PLANNING,
            SourceEntityId = "Anouk.Vantoogh@gmail.com",
            AttendeeUUID = Guid.NewGuid().ToString(),
            EntityType = "SessionAttendee",
            InvitationStatus = NotificationStatus.PENDING,
            SessionUUID = testUuid
        };


        //var session = new PlanningSession("eerste sessie", new DateTime(2022, 12, 01), new DateTime(2022, 12, 2), "Omschrijving van de eerste sessie", testUuid);
        //var sessionAttendee = new PlanningSessionAttendee(MethodEnum.CREATE, testUuid, testUuid, NotificationStatus.PENDING);

        obj = attendee;

            var meh = await pS.ObjectToXml(obj);
            return meh;

    }


    
}

