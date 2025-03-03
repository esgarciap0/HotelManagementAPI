using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationAPI.Models
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        [JsonPropertyName("price")]
        [Precision(18, 2)]// Limita el número de decimales en la base de datos
        public decimal Price { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}