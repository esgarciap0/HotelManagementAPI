using System.Text.Json.Serialization;

namespace HotelReservationAPI.Models.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        //[JsonIgnore]
        public decimal BasePrice { get; set; }
        //[JsonPropertyName("price")]
        public int PriceWithTax => (int)(BasePrice * 1.19m);
        public int PriceRounded => (int)BasePrice;
        public bool IsAvailable { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public int HotelId { get; set; }
        
    }
}
