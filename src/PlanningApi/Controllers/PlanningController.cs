using CalendarServices;
using CalendarServices.Models.Configuration;
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
        public PlanningController(
            ILogger<PlanningController> logger, 
            IGoogleCalendarService calendarService,
            ICalendarOptions calendarOptions)
        {
            this.Logger = logger;
            //this.CalendarOptions = calendarOptions; 
            this.CalendarService = calendarService;
            //CalendarService.CalendarOptions = calendarOptions;
            CalendarService.CreateCalendarService(calendarOptions);
            Logger.LogInformation("PlanningController created");
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
            //var guid = CalendarOptions.CalendarGuid;
            return await CalendarService.GetAllUpcomingSessions(CalendarService.CalendarGuid);
        }

        [HttpPost("RabbitEndpoint")]
        public async Task<IActionResult> RabbitEndpoint([FromBody] string message)
        {
            Logger.LogInformation("RabbitEndpoint called");
            return Ok();
        }

    }
}
