using GardenNetApi.Data;
using GardenNetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace GardenNetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantsController : ControllerBase
    {
        private readonly AppDbContext context;
        public PlantsController(AppDbContext context)
        {
            this.context = context;
        }

        //GET api/plants/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Plant>> GetPlantById(int id)
        {
            var plant = await context.Plants.FindAsync(id);

            if (plant == null)
                return NotFound();

            return plant;
        }

        // GET api/plants
        [HttpGet]
        public IEnumerable<Plant> GetAllPlants() => context.Plants.ToList();

        // POST api/plants
        [HttpPost]
        public async Task<ActionResult<Plant>> PostPlant(Plant plant)
        {
            context.Add(plant);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlantById), new { id = plant.Id }, plant);
        }

        // DELETE api/plants/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlant(int id)
        {
            var plant = await context.Plants.FindAsync(id);
            if (plant == null)
                return NotFound();

            context.Plants.Remove(plant);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlantExists(int id) => context.Plants.Any(m => m.Id == id);
    }
}
