using HotelReservationAPI.Data;
using HotelReservationAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationAPI.Controllers
{
    [Authorize] // Solo accesible para agencias autenticadas
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly HotelContext _context;

        public HotelController(HotelContext context)
        {
            _context = context;
        }

        // Crear un nuevo hotel
        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] Hotel hotel)
        {
            if (hotel == null)
                return BadRequest("Hotel data is required");

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHotelById), new { id = hotel.Id }, hotel);
        }

        //Asignar habitaciones a un hotel
        [HttpPost("{hotelId}/assign-rooms")]
        public async Task<IActionResult> AssignRoomsToHotel(int hotelId, [FromBody] List<int> roomIds)
        {
            var hotel = await _context.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Id == hotelId);
            if (hotel == null) return NotFound("Hotel no encontrado.");

            var rooms = await _context.Rooms.Where(r => roomIds.Contains(r.Id)).ToListAsync();
            if (rooms.Count != roomIds.Count) return BadRequest("Algunas habitaciones no existen.");

            foreach (var room in rooms)
            {
                room.HotelId = hotelId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Habitaciones asignadas al hotel exitosamente." });
        }

        // Modificar valores de un hotel
        [HttpPut("{id}/edit")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] Hotel updatedHotel)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                return NotFound("El hotel no existe.");
            }

            hotel.Name = updatedHotel.Name;
            hotel.City = updatedHotel.City;
            hotel.IsActive = updatedHotel.IsActive;
            hotel.Price = updatedHotel.Price;

            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();

            return Ok(hotel);
        }

        // Habilitar/Deshabilitar un hotel
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleHotelStatus(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) return NotFound("Hotel no encontrado.");

            hotel.IsActive = !hotel.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Estado del hotel actualizado", isActive = hotel.IsActive });
        }

        // Obtener todos los hoteles
        [HttpGet]
        public async Task<IActionResult> GetHotels()
        {
            var hotels = await _context.Hotels
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.City,
                    h.IsActive,
                    Price = (int)h.Price,
                })
                .ToListAsync();

            return Ok(hotels); // Devuelve un JSON con los datos
        }

        // Obtener un hotel por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.Id == id)
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.City,
                    h.IsActive,
                    Price = (int)h.Price, // Redondear precio
                    Rooms = h.Rooms.Select(r => new
                    {
                        r.Id,
                        r.Type,
                        Price = (int)r.Price, // Redondear precio
                        r.IsAvailable,
                        r.Capacity,
                        r.HotelId,
                        r.Reservations
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (hotel == null)
                return NotFound();

            return Ok(hotel);
        }

        // Agregar una habitación a un hotel
        [HttpPost("{hotelId}/add-room")]
        public async Task<ActionResult<Room>> AddRoom(int hotelId, [FromBody] Room room)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null) return NotFound("Hotel no encontrado.");

            room.HotelId = hotelId;
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, room);
        }

        //Eliminar hotel
        [HttpDelete("{hotelId}/delete")]
        public async Task<IActionResult> DeleteHotel(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms) // Incluir habitaciones asociadas
                .FirstOrDefaultAsync(h => h.Id == hotelId);

            if (hotel == null)
                return NotFound("Hotel not found.");

            _context.Rooms.RemoveRange(hotel.Rooms); // Eliminar todas las habitaciones del hotel
            _context.Hotels.Remove(hotel); // Eliminar el hotel

            await _context.SaveChangesAsync();

            return Ok(new { message = "Hotel and all its rooms deleted successfully." });
        }

    }
}