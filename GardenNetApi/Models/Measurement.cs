using System.ComponentModel.DataAnnotations;

namespace GardenNetApi.Models
{
    public enum MeasurementType
    {
        Moisture, Humidity, PAR
    }
    public class Measurement
    {
        public int Id { get; set; }
        [Required]
        public MeasurementType MeasurementType { get; set; }
        public decimal? MeasuredValue { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
    }
}
