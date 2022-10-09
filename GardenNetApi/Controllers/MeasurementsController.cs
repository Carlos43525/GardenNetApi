using GardenNetApi.Data;
using GardenNetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GardenNetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;

        public MeasurementsController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
            this.config = config;
        }

        /// <summary>
        /// Until the MQTT broker is set up and/or the API is live, the api will perform a GET request to thingspeak 
        /// for ESP sensor data. POSTs are not feasible with IIS on local network due to the difficulty and 
        /// inconsistency and difficulty of sending HTTP requests locally. 
        /// 
        /// Current solution: 
        /// Access the thinkspeak URL directly with the injected IHttpClientFactory. The URL returns a JSON that 
        /// is consistent across requests in the form of: 
        ///{
        ///   "channel": {
        ///        "id": 1877019,
        ///        "name": etc...
        ///    },
        ///    "feeds": [
        ///       {
        ///           "created_at": "2022-09-29T05:43:29Z",
        ///           "entry_id": 1,
        ///           "field1": "137"
        ///        },
        ///        {
        /// "created_at": "2022-09-29T05:44:00Z",
        /// "entry_id": 2,
        /// "field1": "221"
        ///        }, 
        /// etc...
        ///
        /// Convert the JSON with ReadAsStringAsync() and then parse the object as neccessary for the object. 
        /// 
        /// For manual parsing: 
        ///     var s = JObject.Parse(contentString);
        ///     var value = s["feeds"][1]["field1"].ToString();
        /// </summary>
        /// <returns></returns>
        /// 
        //GET api/measurements
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<Measurement> GetAllMeasurements() => context.Measurements.ToList(); 

        // GET api/measurments/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Measurement>> GetMeasurementById(int id)
        {
            var measurement = await context.Measurements.FindAsync(id);

            if (measurement == null)
                return NotFound();

            return measurement;
        }

        // POST api/measurements
        [HttpPost]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Measurement>> PostMeasurement(Measurement measurement)
        {
            context.Measurements.AddRange(measurement);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeasurementById), new { id = measurement.Id }, measurement);
        }

        // This method posts the json data from ThingSpeak to the postgres database. This is a short term
        // solution until the microcontrollers can directly post to the server themselves. 
        // POST api/measurements/thingspeak
        [HttpPost("/thinspeak")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Measurement>> PostFromThingSpeak()
        {
            string url = $"https://thingspeak.com/channels/1877019/feeds.json?api_key={config["THING:SPEAK"]}";


            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers =
                {
                    {HeaderNames.Accept, "application/json" }
                }
            };

            var httpClient = httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(request); // Blocking code

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string contentString = await httpResponseMessage.Content.ReadAsStringAsync();

                List<Measurement> measurementsList = new List<Measurement>();

                var options = new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString |
                     JsonNumberHandling.WriteAsString
                };

                JsonDocument doc = JsonDocument.Parse(contentString);

                var parsedJson = doc.RootElement.GetProperty("feeds");

                for (int i = 0; i < parsedJson.GetArrayLength(); i++)
                {
                    Measurement? measurement = JsonSerializer.Deserialize<Measurement>(parsedJson[i], options);
                    measurement.MeasurementType = MeasurementType.Moisture;
                    measurementsList.Add(measurement);
                }

                context.Measurements.AddRange(measurementsList);
                await context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST api/measurements 
        // Test post for ESP32
        [HttpPost("{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> TestPost(int id)
        {
            if (id == null)
                return BadRequest();

            var testMeasurement = new Measurement { Id = id, MeasurementType = 0, MeasuredValue = 33, DateTime = new DateTime() };
            context.Add(testMeasurement);
            await context.SaveChangesAsync();

            return StatusCode(200);

            // return CreatedAtAction(nameof(GetMeasurementById), new { id = testMeasurement.Id }, testMeasurement);
        }

        // PUT api/measurements/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> PutMeasurement(int id, Measurement measurement)
        {
            if (id != measurement.Id)
                return BadRequest();

            context.Entry(measurement).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeasurementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/measurements/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteMeasurement(int id)
        {
            var measurement = await context.Measurements.FindAsync(id);
            if (measurement == null)
                return NotFound();

            context.Measurements.Remove(measurement);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeasurementExists(int id) => context.Measurements.Any(m => m.Id == id);
    }
}