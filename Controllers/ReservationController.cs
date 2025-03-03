using HotelReservationAPI.Data;
using HotelReservationAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace HotelReservationAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly EmailService _emailService;

        public ReservationController(HotelContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        //BUSCAR HOTELES DISPONIBLES CON FILTROS
        [AllowAnonymous]
        [HttpPost("search-hotels")]
        public async Task<ActionResult<IEnumerable<object>>> SearchHotels([FromBody] HotelSearchDto searchRequest)
        {
            if (string.IsNullOrEmpty(searchRequest.City))
                return BadRequest(new { error = "La ciudad es obligatoria." });

            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.City.ToLower() == searchRequest.City.ToLower()) 
                .ToListAsync();

            var availableHotels = hotels
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.City,
                    h.IsActive,
                    Price = Math.Round(h.Price, 0),
                    Rooms = h.Rooms
                        .Where(r => r.IsAvailable && r.Capacity == searchRequest.Guests) 
                        .Select(r => new
                        {
                            r.Id,
                            r.Type,
                            Price = Math.Round(r.Price, 0),
                            r.Capacity,
                            r.IsAvailable,
                            r.HotelId
                        })
                        .ToList()
                })
                .Where(h => h.Rooms.Any()) 
                .ToList();

            if (!availableHotels.Any())
                return NotFound(new { message = "No hay hoteles con habitaciones disponibles para los filtros seleccionados." });

            return Ok(availableHotels);
        }

        // 🔹 OBTENER HABITACIONES DE UN HOTEL ESPECÍFICO
        [AllowAnonymous]
        [HttpGet("hotel/{hotelId}/rooms")]
        public async Task<ActionResult<IEnumerable<Room>>> GetHotelRooms(int hotelId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId && r.IsAvailable)
                .ToListAsync();

            if (!rooms.Any()) return NotFound("No hay habitaciones disponibles en este hotel.");
            return Ok(rooms);
        }

        // CREAR RESERVA
        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            var hotel = await _context.Hotels.FindAsync(reservationDto.HotelId);
            if (hotel == null) return NotFound("El hotel no fue encontrado.");

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == reservationDto.RoomId && r.HotelId == reservationDto.HotelId);
            if (room == null) return NotFound("La habitación no fue encontrada en este hotel.");

            if (!room.IsAvailable) return BadRequest("La habitación no está disponible.");

            if (string.IsNullOrEmpty(reservationDto.PassengerName) || string.IsNullOrEmpty(reservationDto.Email))
                return BadRequest("El nombre y correo del pasajero son obligatorios.");

            if (string.IsNullOrEmpty(reservationDto.EmergencyContactName) || string.IsNullOrEmpty(reservationDto.EmergencyContactPhone))
                return BadRequest("El contacto de emergencia es obligatorio.");

            var reservation = new Reservation
            {
                RoomId = reservationDto.RoomId,
                PassengerName = reservationDto.PassengerName,
                CheckIn = reservationDto.CheckIn,
                CheckOut = reservationDto.CheckOut,
                DateOfBirth = reservationDto.DateOfBirth,
                Gender = reservationDto.Gender,
                DocumentType = reservationDto.DocumentType,
                DocumentNumber = reservationDto.DocumentNumber,
                Email = reservationDto.Email,
                PhoneNumber = reservationDto.PhoneNumber,
                EmergencyContactName = reservationDto.EmergencyContactName,
                EmergencyContactPhone = reservationDto.EmergencyContactPhone
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            string emailBody = $@"
    <h2>Confirmación de Reserva</h2>
    <p>Hola {reservation.PassengerName},</p>
    <p>Tu reserva ha sido confirmada para la habitación <b>{room.Type}</b> en el hotel <b>{hotel.Name}</b>.</p>
    <p><b>Detalles de la reserva:</b></p>
    <ul>
        <li>Fecha de Check-in: {reservation.CheckIn:yyyy-MM-dd}</li>
        <li>Fecha de Check-out: {reservation.CheckOut:yyyy-MM-dd}</li>
        <li>Capacidad: {room.Capacity} personas</li>
    </ul>
    <p>En caso de emergencia, tu contacto registrado es: {reservation.EmergencyContactName} - {reservation.EmergencyContactPhone}</p>
    <p>¡Gracias por reservar con nosotros!</p>";

            _emailService.SendEmail(reservation.Email, "Confirmación de Reserva", emailBody);

            return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, new
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                Room = new
                {
                    room.Id,
                    room.Type,
                    Price = Math.Round(room.Price, 0), 
                    room.IsAvailable,
                    room.Capacity,
                    room.HotelId,
                    Hotel = new
                    {
                        hotel.Id,
                        hotel.Name,
                        hotel.City,
                        hotel.IsActive,
                        Price = Math.Round(hotel.Price, 0) 
                    }
                }
            });
        }


        //OBTENER DETALLE DE UNA RESERVA
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetReservationById(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(room => room!.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound("Reserva no encontrada.");

            var formattedReservation = new
            {
                reservation.Id,
                reservation.RoomId,
                Room = new
                {
                    reservation.Room!.Id,
                    reservation.Room.Type,
                    Price = Math.Round(reservation.Room.Price, 0), 
                    reservation.Room.IsAvailable,
                    reservation.Room.Capacity,
                    reservation.Room.HotelId,
                    Hotel = new
                    {
                        reservation.Room.Hotel!.Id,
                        reservation.Room.Hotel.Name,
                        reservation.Room.Hotel.City,
                        reservation.Room.Hotel.IsActive,
                        Price = Math.Round(reservation.Room.Hotel.Price, 0) 
                    }
                },
                reservation.PassengerName,
                reservation.CheckIn,
                reservation.CheckOut
            };

            return Ok(formattedReservation);
        }


        //LISTAR RESERVAS DE UN HOTEL
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetReservationsByHotel(int hotelId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.Room!.HotelId == hotelId)
                .Include(r => r.Room)
                .ThenInclude(room => room!.Hotel)
                .ToListAsync();

            if (!reservations.Any())
                return NotFound("No se encontraron reservas para este hotel.");

            var formattedReservations = reservations.Select(r => new
            {
                Id = r.Id,
                RoomId = r.RoomId,
                Room = new
                {
                    Id= r.Room!.Id,
                    r.Room.Type,
                    Price = Math.Round(r.Room.Price, 0),
                    r.Room.IsAvailable,
                    r.Room.Capacity,
                    r.Room.HotelId,
                    Hotel = new
                    {
                        Id = r.Room.Hotel!.Id,
                        r.Room.Hotel.Name,
                        r.Room.Hotel.City,
                        r.Room.Hotel.IsActive,
                        Price = Math.Round(r.Room.Hotel.Price, 0) 
                    }
                }
            });

            return Ok(formattedReservations);
        }

    }
}
