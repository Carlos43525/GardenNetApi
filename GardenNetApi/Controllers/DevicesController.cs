using GardenNetApi.Data;
using GardenNetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GardenNetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext context;
        public DevicesController(AppDbContext context)
        {
            this.context = context;
        }

        // GET api/devices/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Device>> GetDeviceById(int id)
        {
            var device = await context.Devices.FindAsync(id);

            if (device == null)
                return NotFound();

            return device;
        }

        // GET api/devices
        [HttpGet]
        public IEnumerable<Device> GetAllDevices() => context.Devices.ToList();

        // POST api/devices
        [HttpPost]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            context.Add(device);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
        }

        // DELETE api/devices/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            context.Devices.Remove(device);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeviceExists(int id) => context.Devices.Any(m => m.Id == id);
    }
}
