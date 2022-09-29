using System.ComponentModel.DataAnnotations;

namespace GardenNetApi.Models
{
    public enum PlantType
    {
        HousePlant, GardenPlant
    }

    public class Plant
    {
        public int Id { get; set; }
        // The common name for the plant
        [Required]
        public string Name { get; set; }

        // The binomial name for a given plant 
        public string? ScientificName { get; set; }
        [Required]
        public PlantType PlantType { get; set; }
        public string? Location { get; set; }
        // Date planted or acquired
        [Required]
        public DateTime Date { get; set; }

    }
}
