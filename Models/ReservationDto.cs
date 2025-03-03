namespace HotelReservationAPI.Models.DTOs
{
    public class ReservationDto
    {
        public int HotelId { get; set; } 
        public int RoomId { get; set; }  
        public DateTime CheckIn { get; set; } 
        public DateTime CheckOut { get; set; } 

        
        public string PassengerName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }
}
