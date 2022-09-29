using GardenNetApi.Data;
using GardenNetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GardenNetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/devices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDeviceById(int id)
        {
            var device = await _context.Devices.FindAsync(id);

            if (device == null)
                return NotFound();

            return device;
        }

        // GET api/devices
        [HttpGet]
        public IEnumerable<Device> GetAllDevices() => _context.Devices.ToList();

        // POST api/devices
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            _context.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
        }

        // DELETE api/devices/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeviceExists(int id) => _context.Devices.Any(m => m.Id == id);
    }
}
