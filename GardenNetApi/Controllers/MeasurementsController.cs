using GardenNetApi.Data;
using GardenNetApi.Models;
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
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public MeasurementsController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
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
        public IEnumerable<Measurement> GetAllMeasurements() => _context.Measurements.ToList(); 

        //// GET api/measurments
        //[HttpGet]
        ////public IEnumerable<Measurement> GetAllMeasurements() => _context.Measurements.ToList();

        //public async Task DepricatedGetAllMeasurements()
        //{
        //    //JsonDocument doc = JsonDocument.Parse(jsonString);

        //    //var parsedJson = doc.RootElement.GetProperty("feeds")[0];

        //    //Measurement? measurement = JsonSerializer.Deserialize<Measurement>(parsedJson);

        //    //measurement.Id = 44;
        //    //measurement.MeasurementType = MeasurementType.Moisture; 

        //    //Console.WriteLine($"Id: {measurement.Id}");
        //    //Console.WriteLine($"Type: {measurement.MeasurementType}");
        //    //Console.WriteLine($"Value: {measurement.MeasuredValue}");
        //    //Console.WriteLine($"Date: {measurement.DateTime}");

        //    //_context.Add(measurement);
        //    //await _context.SaveChangesAsync();

        //    string url = "https://thingspeak.com/channels/1877019/feeds.json?api_key=SD9V582YZUA38AM7";

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get, url)
        //    {
        //        Headers =
        //        {
        //            {HeaderNames.Accept, "application/json" }
        //        }
        //    };

        //    var httpClient = _httpClientFactory.CreateClient();
        //    var httpResponseMessage = await httpClient.SendAsync(request); // Blocking code

        //    if (httpResponseMessage.IsSuccessStatusCode)
        //    {
        //        string contentString = await httpResponseMessage.Content.ReadAsStringAsync();

        //        JsonDocument doc = JsonDocument.Parse(contentString);

        //        var parsedJson = doc.RootElement.GetProperty("feeds")[0];


        //        //var s = JObject.Parse(contentString);
        //        //var value = s["feeds"][1]["field1"].ToString();
        //        //Console.WriteLine(value);

        //        //Console.WriteLine(contentString);

        //        //_context.Add(measurement);
        //        //await _context.SaveChangesAsync();
        //    }
        //}

        // GET api/measurments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Measurement>> GetMeasurementById(int id)
        {
            var measurement = await _context.Measurements.FindAsync(id);

            if (measurement == null)
                return NotFound();

            return measurement;
        }

        // POST api/measurements
        [HttpPost]
        public async Task<ActionResult<Measurement>> PostMeasurement(Measurement measurement)
        {
            _context.Measurements.AddRange(measurement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeasurementById), new { id = measurement.Id }, measurement);
        }

        // This method posts the json data from ThingSpeak to the postgres database. This is a short term
        // solution until the microcontrollers can directly post to the server themselves. 
        // POST api/measurements/thingspeak
        [HttpPost("/thinspeak")]
        public async Task<ActionResult<Measurement>> PostFromThingSpeak()
        {
            string url = $"https://thingspeak.com/channels/1877019/feeds.json?api_key={_config["ThingSpeak:ApiKey"]}";


            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers =
                {
                    {HeaderNames.Accept, "application/json" }
                }
            };

            var httpClient = _httpClientFactory.CreateClient();
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

                _context.Measurements.AddRange(measurementsList);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST api/measurements 
        // Test post for ESP32
        [HttpPost("{id}")]
        public async Task<IActionResult> TestPost(int id)
        {
            if (id == null)
                return BadRequest();

            var testMeasurement = new Measurement { Id = id, MeasurementType = 0, MeasuredValue = 33, DateTime = new DateTime() };
            _context.Add(testMeasurement);
            await _context.SaveChangesAsync();

            return StatusCode(200);

            // return CreatedAtAction(nameof(GetMeasurementById), new { id = testMeasurement.Id }, testMeasurement);
        }

        // PUT api/measurements/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeasurement(int id, Measurement measurement)
        {
            if (id != measurement.Id)
                return BadRequest();

            _context.Entry(measurement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> DeleteMeasurement(int id)
        {
            var measurement = await _context.Measurements.FindAsync(id);
            if (measurement == null)
                return NotFound();

            _context.Measurements.Remove(measurement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeasurementExists(int id) => _context.Measurements.Any(m => m.Id == id);
    }
}
