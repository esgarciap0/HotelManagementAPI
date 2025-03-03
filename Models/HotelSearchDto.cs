namespace HotelReservationAPI.Models.DTOs
{
    public class HotelSearchDto
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }
        public string City { get; set; } = string.Empty;
    }
}
