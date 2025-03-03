using HotelReservationAPI.Data;
using HotelReservationAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace HotelReservationAPI.Controllers
{
    [Authorize] // Solo accesible para agencias autenticadas
    [Route("api/rooms")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomController(HotelContext context)
        {
            _context = context;
        }

        //Obtener todas las habitaciones de un hotel
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRoomsByHotel(int hotelId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .Include(r => r.Hotel)
                .Select(r => new
                {
                    r.Id,
                    r.Type,
                    Price = (int)r.Price,
                    r.IsAvailable,
                    r.HotelId,
                    r.Capacity,
                    Hotel = r.Hotel != null ? r.Hotel.Name : "No asignado"
                })
                .ToListAsync();

            return Ok(rooms);
        }

        //  Crear una habitación en un hotel
        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] RoomDto roomDto)
        {
            var hotel = await _context.Hotels.FindAsync(roomDto.HotelId);
            if (hotel == null) return NotFound("El hotel no existe.");

            
            var room = new Room
            {
                Type = roomDto.Type,
                Price = Math.Round(roomDto.BasePrice * 1.19m, 0), 
                IsAvailable = roomDto.IsAvailable,
                Capacity = roomDto.Capacity,
                HotelId = roomDto.HotelId,
                Location = roomDto.Location 
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoomsByHotel), new { hotelId = room.HotelId }, room);
        }

        // Modificar los valores de una habitación
        [HttpPut("hotel/{hotelId}/room/edit")]
        public async Task<IActionResult> EditRoom(int hotelId, [FromBody] RoomDto updatedRoomDto)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == updatedRoomDto.Id && r.HotelId == hotelId);
            if (room == null)
                return NotFound("Room not found.");

            
            room.Type = updatedRoomDto.Type;
            room.Price = Math.Round(updatedRoomDto.BasePrice * 1.19m, 0); 
            room.IsAvailable = updatedRoomDto.IsAvailable;
            room.Capacity = updatedRoomDto.Capacity;
            room.Location = updatedRoomDto.Location ?? room.Location; 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Room updated successfully",
                room
            });
        }

        // Habilitar o Deshabilitar una habitación
        [HttpPatch("hotel/{hotelId}/room/{roomId}/toggle-status")]
        public async Task<IActionResult> ToggleRoomStatus(int hotelId, int roomId)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.HotelId == hotelId);

            if (room == null)
            {
                return NotFound(new { Error = "Room not found for this hotel." });
            }

            
            room.IsAvailable = !room.IsAvailable;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Room status updated successfully", IsAvailable = room.IsAvailable });
        }

        //Eliminar habitacion de un hotel
        [HttpDelete("hotel/{hotelId}/room/{roomId}/delete")]
        public async Task<IActionResult> DeleteRoom(int hotelId, int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Reservations) 
                .FirstOrDefaultAsync(r => r.Id == roomId && r.HotelId == hotelId);

            if (room == null)
                return NotFound("Room not found.");

            if (room.Reservations.Any())
                return BadRequest("Cannot delete room with active reservations.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room deleted successfully." });
        }

    }
}






