using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

class Program
{
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/calendar-dotnet-quickstart.json

    //Scope aanpassen indien nodig ivm veiligheid. Staat nu open. 
    static string[] Scopes = {
        //CalendarService.Scope.CalendarReadonly,
        CalendarService.Scope.Calendar,
        CalendarService.Scope.CalendarEvents,
        CalendarService.Scope.CalendarSettingsReadonly
    };

    static string ApplicationName = "Google Calendar API Test"; //dubbelchecken in gCloud services.
    static string CalendarId = "planning.integrationproject.ehb@gmail.com"; //TODO: uit AppSettings halen.
    static string eventIdMeeting120322 = "0pmjplojtl1hsp897u8s2shns4";      //TODO: uit AppSettings halen.

    static void Main(string[] args)
    {
        UserCredential credential;

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

        // Create Google Calendar API service with the credential.
        var service = new CalendarService(new BaseClientService.Initializer()
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
            },
            Source = new()
            {
                Title = "Console App als test"
            }
        };


        //service.Events.Insert(newEvent, CalendarId).Execute();
        ;

        //EventsResource.InsertRequest request = new EventsResource.InsertRequest(service, newEvent, CalendarId);
        //Event response = request.Execute();

        //service.Events.Insert(newEvent, CalendarId).Execute();
        //;


        //Add an Attendee
        EventAttendee newAttendee = new()
        {
            Email = "EersteGenodigde@gmail.com",
            DisplayName = "Eerste Genodigde",
            Comment = "Ik zal zeker komen"
        };
        var eventItem = GetEvent(service, eventIdMeeting120322).Result; //.ConfigureAwait(false);
        if (eventItem is not null)
            AddAttendee(service, eventItem, newAttendee).ConfigureAwait(false);



    }

    async static Task<Event?> GetEvent(CalendarService service, string eventId)
    {
        EventsResource.ListRequest request = service.Events.List("primary");
        //var request = service.Events.Get(CalendarId, eventId);
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
                        Console.WriteLine($"{attendee.DisplayName} ({attendee.Email}) - Antwoord: {attendee.ResponseStatus}");
                    }
            }
        }
        else
        {
            Console.WriteLine("No upcoming events found.");
        }


    }


    async static Task AddAttendee(CalendarService service, Event eventItem, EventAttendee eventAttendee)
    {
        eventItem.Attendees.Add(eventAttendee);
        var updatedEvent = await service.Events.Update(eventItem, CalendarId, eventIdMeeting120322).ExecuteAsync();

        Console.WriteLine($"aantal genodigden: {updatedEvent.Attendees?.Count().ToString() ?? "unavailable"}");
    }



}

