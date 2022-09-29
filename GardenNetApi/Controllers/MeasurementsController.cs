using GardenNetApi.Data;
using GardenNetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GardenNetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MeasurementsController(AppDbContext context)
        {
            this._context = context;
        }

        // GET api/measurments
        [HttpGet]
        public IEnumerable<Measurement> GetAllMeasurements() => _context.Measurements.ToList();

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
            _context.Add(measurement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeasurementById), new { id = measurement.Id }, measurement);
        }

        // POST api/measurements 
        // Test post for ESP32
        [HttpPost("{id}")]
        public async Task<IActionResult> TestPost(int id)
        {
            if (id == null)
                return BadRequest(); 

            var testMeasurement = new Measurement { Id = id, MeasurementType = 0, MeasuredValue = 33, DateTime = new DateTime()};
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
