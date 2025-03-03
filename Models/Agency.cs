using Microsoft.AspNetCore.Identity;

namespace HotelReservationAPI.Models
{
    public class Agency : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}

