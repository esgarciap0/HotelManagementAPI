using Microsoft.AspNetCore.Identity;

namespace HotelReservationAPI.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}