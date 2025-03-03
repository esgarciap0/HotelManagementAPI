namespace HotelReservationAPI.Models.DTOs
{
    public class ReservationDto
    {
        public int HotelId { get; set; } // ID del hotel
        public int RoomId { get; set; }  // ID de la habitación
        public DateTime CheckIn { get; set; } // Fecha de entrada
        public DateTime CheckOut { get; set; } // Fecha de salida

        // Datos del pasajero
        public string PassengerName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Contacto de emergencia
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }
}
