using Microsoft.AspNetCore.Mvc;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CalendarWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        /*
            TODO: na deployen in een container op EHB de API calendar key restricten op IP adres van de server. 

        */


        //google calendar api call to get events




        // GET: api/<CalendarController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CalendarController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CalendarController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CalendarController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CalendarController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
