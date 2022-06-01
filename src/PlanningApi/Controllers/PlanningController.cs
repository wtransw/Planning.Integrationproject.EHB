using CalendarServices;
using CalendarServices.Models;
using CalendarServices.Models.Configuration;
using Crm.Link.RabbitMq.Producer;
using Microsoft.AspNetCore.Mvc;
using PlanningApi.Configuration;

namespace PlanningApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanningController : ControllerBase
    {
        private readonly ILogger<PlanningController> Logger;
        private readonly IGoogleCalendarService CalendarService;
        //private readonly ICalendarOptions CalendarOptions;
        private readonly PlanningAttendeePublisher PlanningAttendeePublisher;
        private readonly PlanningSessionPublisher PlanningSessionPublisher;
        private readonly PlanningSessionAttendeePublisher PlanningSessionAttendeePublisher;

        private readonly PlanningService PlanningService;
        public PlanningController(
            ILogger<PlanningController> logger, 
            IGoogleCalendarService calendarService,
            ICalendarOptions calendarOptions,
            PlanningAttendeePublisher planningAttendeePublisher,
            PlanningSessionPublisher planningSessionPublisher,
            PlanningSessionAttendeePublisher planningSessionAttendeePublisher)
            //PlanningService planningService)
        {
            this.Logger = logger;
            //this.CalendarOptions = calendarOptions; 
            this.CalendarService = calendarService;
            calendarService.CreateCalendarService(calendarOptions);
            //CalendarService.CalendarGuid = calendarOptions.CalendarGuid;
            Logger.LogInformation("PlanningController created");
            this.PlanningAttendeePublisher = planningAttendeePublisher;
            this.PlanningSessionPublisher = planningSessionPublisher;
            this.PlanningSessionAttendeePublisher = planningSessionAttendeePublisher;
            //this.PlanningService = planningService;
        }


        //[HttpGet(Name = "Meh")]
        [HttpGet("Test")]
        public string Get()
        {
            return "Hello from PlanningController";
        }


        [HttpGet("GetSessions")]    // API/Planning/Getsessions
        public async Task<List<Google.Apis.Calendar.v3.Data.Event>> GetUpcomingSessions()
        {
            try
            {
                return await CalendarService.GetAllUpcomingSessions(CalendarService.CalendarGuid);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogError(ex.Message);
                return new List<Google.Apis.Calendar.v3.Data.Event>();
            }
            //var guid = CalendarOptions.CalendarGuid;
        }

        [HttpGet("HandleAttendeeTest")]    // API/Planning/HandleAttendee
        public async Task HandleAttendeeTest(string naam, string email, string? vatNumber)
        {
            try
            {
                var testAttendee = new Google.Apis.Calendar.v3.Data.EventAttendee() { Email = email, DisplayName = naam };

                var attendeeUitGoogleCalendar = await CalendarService.GetAttendeeByEmail(testAttendee.Email);

                if (attendeeUitGoogleCalendar == null)      //Rogerke aanmaken
                {
                    var upcomingSessions =  await CalendarService.GetAllUpcomingSessions(CalendarService.CalendarGuid);
                    var firstSession = upcomingSessions.FirstOrDefault();
                    if (firstSession != null)
                    {
                        var eventWithAddedAttendee = await CalendarService.AddAttendeeToSessionAsync(firstSession.Id, testAttendee);
                        var addedAttendee = eventWithAddedAttendee.Attendees.First(a => a.DisplayName == naam);
                        Console.WriteLine(naam + " is aangemaakt en toegevoegd aan " + firstSession.Description + " en heeft id " + addedAttendee.Id);
                    }
                }

                else //Rogerke updaten
                {
                    attendeeUitGoogleCalendar.DisplayName = naam; 
                    if (!string.IsNullOrEmpty(vatNumber))
                        attendeeUitGoogleCalendar.Comment = vatNumber;

                    await CalendarService.UpdateAttendee(attendeeUitGoogleCalendar);
                }
                Console.WriteLine("Rogerke is updated");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogError(ex.Message);
            }
            //var guid = CalendarOptions.CalendarGuid;
        }


        //[HttpPost("RabbitEndpoint")]
        //public async Task<IActionResult> RabbitEndpoint([FromBody] string message)
        //{
        //    Logger.LogInformation("RabbitEndpoint called");
        //    return Ok();
        //}

        [HttpPost("PublishOnRabbitMqTest")]
        public async Task<IActionResult> PublishOnRabbitMqTest(int objectNumber)
        {
            object obj;
            string testUuidSession = "12345678901234567890123456789012";
            string testUuidAttendee = "12345678901234567890123456789015";
            string testUuidAttendeeGuest = "23456789012345678901234567890156";

            var attendee = new PlanningAttendee()
            {
                Email = "no@email.yet",
                EntityVersion = 1,
                Name = "Jean",
                LastName = "Avec la casquette",
                Method = MethodEnum.CREATE,
                Source = SourceEnum.PLANNING,
                SourceEntityId = "no@email.yet",
                UUID_Nr = testUuidAttendee,
                EntityType = "Attendee"
            };
            var session = new PlanningSession()
            {
                Title = "Test sessie Sinterklaas",
                StartDateUTC = new DateTime(2022, 12, 06,08,00,00),
                EndDateUTC = new DateTime(2022, 12, 06, 17,00,00),
                EntityType = "SessionEvent",
                IsActive = true,
                EntityVersion = 1,
                Method = MethodEnum.CREATE,
                OrganiserUUID = testUuidAttendee,
                SourceEntityId = "testUuid",
                Source = SourceEnum.PLANNING,
                UUID_Nr = testUuidSession,
            };
            var sessionAttendee = new PlanningSessionAttendee()
            {
                UUID_Nr = Guid.NewGuid().ToString(),
                EntityVersion = 1,
                Method = MethodEnum.CREATE,
                Source = SourceEnum.PLANNING,
                SourceEntityId = "no@email.yet",
                AttendeeUUID = testUuidAttendee,
                EntityType = "SessionAttendee",
                InvitationStatus = NotificationStatus.PENDING,
                SessionUUID = testUuidSession,
            };

            var attendeeGuest = new PlanningAttendee()
            {
                Email = "Benny@gmail.meh",
                EntityVersion = 1,
                Name = "Benny",
                LastName = "Van Ghisteren",
                Method = MethodEnum.CREATE,
                Source = SourceEnum.PLANNING,
                SourceEntityId = "Benny@gmail.meh",
                UUID_Nr = testUuidAttendeeGuest,
                EntityType = "Attendee"
            };
            var sessionAttendeeGuest = new PlanningSessionAttendee()
            {
                UUID_Nr = Guid.NewGuid().ToString(),
                EntityVersion = 1,
                Method = MethodEnum.CREATE,
                Source = SourceEnum.PLANNING,
                SourceEntityId = "Benny@gmail.meh",
                AttendeeUUID = testUuidAttendeeGuest,
                EntityType = "SessionAttendee",
                InvitationStatus = NotificationStatus.PENDING,
                SessionUUID = testUuidSession
            };

            var sessionAttendeeGuestAltered = new PlanningSessionAttendee()
            {
                UUID_Nr = Guid.NewGuid().ToString(),
                EntityVersion = 2,
                Method = MethodEnum.UPDATE,
                Source = SourceEnum.PLANNING,
                SourceEntityId = "Benny@gmail.meh",
                AttendeeUUID = testUuidAttendeeGuest,
                EntityType = "SessionAttendee",
                InvitationStatus = NotificationStatus.ACCEPTED,
                SessionUUID = testUuidSession
            };

            try
            {
                switch (objectNumber)
                {
                    case 1:
                        PlanningSessionPublisher.Publish(session);
                        break;
                    case 2:
                        PlanningAttendeePublisher.Publish(attendee);
                        break;
                    case 3:
                        PlanningSessionAttendeePublisher.Publish(sessionAttendee);
                        break;
                    case 4:
                        PlanningAttendeePublisher.Publish(attendeeGuest);
                        break;
                    case 5:
                        PlanningSessionAttendeePublisher.Publish(sessionAttendeeGuest);
                        break;
                    case 6:
                        PlanningSessionAttendeePublisher.Publish(sessionAttendeeGuestAltered);
                        break;
                    default:
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError($"couldn't publish object of type number {objectNumber}: {ex.Message}");
            }
            return BadRequest();

        }


        //[HttpGet("PublishTest")]    // API/Planning/PublishTest
        //public IActionResult PublishTest()
        //{
        //    try
        //    {
        //        //var attendee = new PlanningAttendee { Name = "Wouter", LastName = "A", Email = "my@mail.here", EntityVersion = "1", UUID_Nr = Guid.NewGuid().ToString(), Method = MethodEnum.CREATE, EntityType = "Attendee", Source = Source.PLANNING, SourceEntityId = "brol@mail.be" };
        //        var attendee = new PlanningAttendee { 
        //            Name = "Wouter", 
        //            LastName = "A", 
        //            Email = "my@mail.here", 
        //            EntityVersion = 1, 
        //            UUID_Nr = Guid.NewGuid().ToString(), 
        //            Method = MethodEnum.CREATE, 
        //            Source = SourceEnum.PLANNING, 
        //            SourceEntityId = "my@mail.here"
        //        }; 


        //        planningAttendeePublisher.Publish(attendee);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Logger.LogError(ex.Message);
        //        return UnprocessableEntity(ex);
        //    }
        //    //var guid = CalendarOptions.CalendarGuid;
        //}










        [HttpPost("AttendeeEventDebug")]
        public async Task<IActionResult> AttendeeEventDebug(int objectNumber)
        {

            try
            {
                var planningAttendee = new PlanningAttendee()
                {
                    Name = "Mathieu",
                    LastName = "Tulpinck",
                    Email = "mathieu.tulpinck@hackaton2022.test",
                    UUID_Nr = "7f8a265b-9e2c-4351-a373-c3db95470b68",
                    EntityType = "ATTENDEE"
                };
                var eventAttendee = new Google.Apis.Calendar.v3.Data.EventAttendee()
                {
                    //Id = planningAttendee.Email,
                    DisplayName = planningAttendee.LastName + planningAttendee.Name,
                    Email = planningAttendee.Email,
                    Comment = planningAttendee.UUID_Nr ?? "",
                    Organizer = planningAttendee.EntityType.ToLower().Contains("org"),
                };

                var planningSessionAttendee = new PlanningSessionAttendee()
                {
                    AttendeeUUID = "7f8a265b-9e2c-4351-a373-c3db95470b68",
                    EntityType = "SESSIONATTENDEE",
                    EntityVersion = 1,
                    InvitationStatus = NotificationStatus.ACCEPTED,
                    Method = MethodEnum.CREATE,
                    SessionUUID = "9e5c9d00-9fb6-411d-99c6-259132203d06",
                    Source = SourceEnum.FRONTEND,
                    SourceEntityId = "18",
                    UUID_Nr = "21834faf-7df2-4e8e-b2b0-b66bf565de15"
                };

                if (objectNumber == 2)
                    await HandleAttendee(planningAttendee);
                else
                    await HandlePlanningSessionAttendee(planningSessionAttendee);

                return Ok();

            }
            catch (Exception ex)
            {
                Logger.LogError("'t is kapot...{msg}", ex.Message);
                return BadRequest();
            }


        }








        // DEBUG


        private async Task HandleAttendee(PlanningAttendee planningAttendee)
        {
            var maxRetries = 5;
            Logger.LogInformation($"Handling planning attendee {planningAttendee.Email}");

            //Kijken welke versie wij hebben van dit object.
            //var uuidData = await UuidMaster.GetGuid(planningAttendee.Email, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Attendee);

            Crm.Link.UUID.Model.ResourceDto uuidData = new();

            //enkel afhandelen als de versienummer hoger is dan wat al bestond. 
            if (uuidData != null && uuidData.EntityVersion > 0 && uuidData.EntityVersion < planningAttendee.EntityVersion)
            {
                try
                {
                    await UpdateAttendeeInGoogleCalendar(planningAttendee);
                    //await UuidMaster.UpdateEntity(planningAttendee.Email, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Attendee);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error while handling Attendee {planningAttendee.Email}: {ex.Message}", ex);
                }
            }


            // We krijgen een Attendee binnen die nog niet bestaat. We kunnen enkel een attendee toevoegen als we ook een sessie hebben waarin deze bestaat. 
            // We wachten tot er een sessie bestaat met deze attendee er in, en voegen hem dan toe.
            // Opletten: dit kan ook de organizer zijn voor de sessie. 
            else if (uuidData == null)
            {
                //create attendee ALS er een sessionattendee voor dit object bestaat, eventueel met een retry over paar min? 
                //Of als de dummy al gemaakt is in Google Calendar.

                for (int i = 0; i < maxRetries; i++)
                {
                    //bij create: in google de sessies ophalen, en kijken of ik sessie heb met deze attendee. 
                    //Dat moet per definitie de dummy zijn, want het is een Create. 
                    // -> deze aanpassen. 

                    //Als we al een sessionAttendee gekregen hebben vanuit de queue, dan bestaat ie in google calendar, dus kunnen we ook gewoon updaten.
                    try
                    {
                        var dummyAttendee = await CalendarService.GetAttendeeByUuid(planningAttendee.UUID_Nr);
                        if (dummyAttendee != null)
                        {
                            //if (planningAttendee.EntityType.ToLower().Contains("org"))
                            //{
                            //    await CalendarService.CreateOrganizer()
                            //}
                            //else
                                await UpdateAttendeeInGoogleCalendar(planningAttendee);
                                //await UuidMaster.PublishEntity(SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Attendee, planningAttendee.Email, planningAttendee.EntityVersion);
                            i = maxRetries;
                        }
                        else
                        {
                            //await Task.Delay(5 * 60 * 1000).ContinueWith(async t =>
                            //    uuidData = await UuidMaster.GetGuid(planningAttendee.Email, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Attendee));

                            if (uuidData != null && uuidData.EntityVersion < planningAttendee.EntityVersion)
                            {
                                await UpdateAttendeeInGoogleCalendar(planningAttendee);
                                i = maxRetries;
                            }
                        }

                        i++;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error while handling Attendee {planningAttendee.Email}: {ex.Message}", ex);
                    }

                }

            }

            ;
        }

        private async Task UpdateAttendeeInGoogleCalendar(PlanningAttendee planningAttendee)
        {
            var eventAttendee = new Google.Apis.Calendar.v3.Data.EventAttendee()
            {
                Id = planningAttendee.Email,
                DisplayName = planningAttendee.LastName + planningAttendee.Name,
                Email = planningAttendee.Email,
                Comment = planningAttendee.UUID_Nr ?? "",
                Organizer = planningAttendee.EntityType.ToLower().Contains("org")
            };

            await CalendarService.UpdateAttendee(eventAttendee);

        }

        private async Task HandlePlanningSessionAttendee(PlanningSessionAttendee planningSessionAttendee)
        {
            Logger.LogInformation($"Handling planning Session attendee {planningSessionAttendee.AttendeeUUID}");
            var maxRetries = 10;

            //haal de session op (met retries)
            //update de attendee voor die session, of create hem.
            for (int i = 0; i < maxRetries; i++)
            {

                var session = await CalendarService.GetSession(null, planningSessionAttendee.SessionUUID);

                if (session != null)
                {
                    Logger.LogInformation($"Sessie {session.Description} gevonden.");
                    //update 
                    try
                    {
                        //kijk of er in de attendees van de sessie al 1 staat met deze guid, en update hem
                        var attendee = session.Attendees.FirstOrDefault(a => a.Comment == planningSessionAttendee.AttendeeUUID);
                        if (attendee != null)
                        {
                            attendee.ResponseStatus = planningSessionAttendee.InvitationStatus.ToString();
                            //await UuidMaster.UpdateEntity(attendee.Email, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Attendee);
                        }
                        else
                        {
                            //maak de attendee met default waardes. We vullen nog geen versienummer in in de UUID master. 
                            //Hierdoor wordt automatisch de rest ingevuld wanneer de attendee van de queue komt. 

                            var responseStatus = (planningSessionAttendee.InvitationStatus.ToString()) switch
                            {
                                "PENDING" => "needsAction",
                                "ACCEPTED" => "accepted",
                                "DECLINED" => "declined",
                                _ => "needsAction"
                            };
                            var nieuweAttendee = new Google.Apis.Calendar.v3.Data.EventAttendee()
                            {
                                Id = planningSessionAttendee.AttendeeUUID,
                                Comment = planningSessionAttendee.AttendeeUUID,
                                DisplayName = "new attendee",
                                Email = "default@email.val",
                                ResponseStatus = responseStatus,
                                Organizer = false
                            };

                            session.Attendees.Add(nieuweAttendee);
                        }

                        await CalendarService.UpdateSession(CalendarService.CalendarGuid, session);
                        i = maxRetries;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error while handling Attendee {planningSessionAttendee.UUID_Nr}: {ex.Message}", ex);
                        i++;
                    }
                }

                // anders 2 minuten wachten, en opnieuw proberen. Eerst moet de sessie aangemaakt worden, en dan pas kunnen we een attendee linken. 
                else
                {
                    Logger.LogError($"Sessie {planningSessionAttendee.SessionUUID} niet gevonden. Wacht 2 min en probeer opnieuw.");
                    await Task.Delay(2 * 60 * 1000);
                }
            }


        }

    }
}
