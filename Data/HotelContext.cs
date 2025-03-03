using HotelReservationAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelReservationAPI.Models.DTOs;

namespace HotelReservationAPI.Data
{
    public class HotelContext : IdentityDbContext<Agency>
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        // Definir las tablas en la base de datos
       //ublic DbSet<Agency> Agencies { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Agency)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AgencyId)
                .HasPrincipalKey(a => a.Id);
        }
    }
}