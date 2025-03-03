using Microsoft.AspNetCore.Identity;

namespace HotelReservationAPI.Models
{
    public class Agency : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        // Relación con Reservaciones
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}

