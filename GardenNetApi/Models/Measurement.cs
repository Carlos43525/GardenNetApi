using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("field1")]
        public decimal? MeasuredValue { get; set; }
        [Required]
        [JsonPropertyName("created_at")]        
        public DateTime DateTime { get; set; }
    }
}
