using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Projekt4.Models;

namespace Projekt4.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nazwa).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Lokalizacja).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => new { e.Nazwa, e.Lokalizacja }).IsUnique();
            });

            // Configure Reservation entity
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cena).HasColumnType("decimal(18,2)");

                // Configure relationship
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.Reservations)
                      .HasForeignKey(e => e.SalaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}