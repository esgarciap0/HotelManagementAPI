using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationAPI.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int Capacity { get; set; }
        public string Location {get; set; } = string.Empty;

        // Relación con Hotel
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        // Relación con Reservaciones
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
