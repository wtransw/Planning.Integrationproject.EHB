﻿using CalendarServices;
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
        private readonly CalendarOptions CalendarOptions;
        private readonly PlanningAttendeePublisher PlanningAttendeePublisher;
        private readonly PlanningSessionPublisher PlanningSessionPublisher;
        private readonly PlanningSessionAttendeePublisher PlanningSessionAttendeePublisher;

        private readonly PlanningService PlanningService;
        public PlanningController(
            ILogger<PlanningController> logger, 
            IGoogleCalendarService calendarService,
            ICalendarOptions calendarOptions)
            CalendarOptions calendarOptions,
            PlanningAttendeePublisher planningAttendeePublisher,
            PlanningSessionPublisher planningSessionPublisher,
            PlanningSessionAttendeePublisher planningSessionAttendeePublisher,
            PlanningService planningService)
        {
            this.Logger = logger;
            this.CalendarOptions = calendarOptions; 
            this.CalendarService = calendarService;
            CalendarService.CalendarGuid = calendarOptions.CalendarGuid;
            Logger.LogInformation("PlanningController created");
            this.PlanningAttendeePublisher = planningAttendeePublisher;
            this.PlanningSessionPublisher = planningSessionPublisher;
            this.PlanningSessionAttendeePublisher = planningSessionAttendeePublisher;
            this.PlanningService = planningService;
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



        [HttpPost("RabbitEndpoint")]
        public async Task<IActionResult> RabbitEndpoint([FromBody] string message)
        {
            Logger.LogInformation("RabbitEndpoint called");
            return Ok();
        }

        [HttpPost("PublishOnRabbitMqTest")]
        public async Task<IActionResult> PublishOnRabbitMqTest(int objectNumber)
        {
            object obj;
            string testUuid = "12345678901234567890123456789012";
            var attendee = new PlanningAttendee { Name = "Wouter", LastName = "A", Email = "my@mail.here", VatNumber = "", Version = 12 };
            var session = new PlanningSession("eerste sessie", new DateTime(2022, 12, 01), new DateTime(2022, 12, 2), "Omschrijving van de eerste sessie", testUuid);
            var sessionAttendee = new PlanningSessionAttendee(MethodEnum.create, testUuid, testUuid, NotificationStatus.pending);

            obj = attendee;

            try
            {

                switch (objectNumber)
                {
                    case 1:
                        PlanningAttendeePublisher.Publish(attendee);
                        break;
                    case 2:
                        PlanningSessionPublisher.Publish(session);
                        break;
                    case 3:
                        PlanningSessionAttendeePublisher.Publish(sessionAttendee);
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


        [HttpGet("PublishTest")]    // API/Planning/PublishTest
        public IActionResult PublishTest()
        {
            try
            {
                //maak een attendee
                //publish dat ding
                var attendee = new PlanningAttendee { Name = "Wouter", LastName = "A", Email = "my@mail.here", VatNumber = "", Version = 12 };
                planningAttendeePublisher.Publish(attendee);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogError(ex.Message);
                return UnprocessableEntity(ex);
            }
            //var guid = CalendarOptions.CalendarGuid;
        }


    }
}
