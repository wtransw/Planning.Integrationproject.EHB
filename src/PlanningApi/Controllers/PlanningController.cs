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

        [HttpGet("HandleAttendee")]    // API/Planning/HandleAttendee
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
                UUID_Nr = testUuidSession
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
                SessionUUID = testUuidSession
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


    }
}
