using Microsoft.EntityFrameworkCore;
using Projekt4.Models;

namespace Projekt4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cena).HasColumnType("decimal(18,2)");
            });

            // Configure Reservation entity
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cena).HasColumnType("decimal(18,2)");

                // Configure relationship
                entity.HasOne(e => e.Sala)
                      .WithMany(r => r.Rezerwacje)
                      .HasForeignKey(e => e.SalaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}