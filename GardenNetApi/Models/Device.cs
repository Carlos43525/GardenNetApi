using System.ComponentModel.DataAnnotations;

namespace GardenNetApi.Models
{
    public enum DeviceType
    {
        ESP8266, ESP32
    }

    public class Device
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public DeviceType DeviceType { get; set; }
        [Required]
        public DateTime DeployDate { get; set; }
    }
}
