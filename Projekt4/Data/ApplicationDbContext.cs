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
        public DbSet<Attachment> Attachments { get; set; }

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

            
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cena).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tytuł).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notatki).HasMaxLength(1000);
                entity.Property(e => e.UserId).IsRequired();

                
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.Reservations)
                      .HasForeignKey(e => e.SalaId)
                      .OnDelete(DeleteBehavior.Restrict);

                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                
                entity.HasIndex(e => new { e.SalaId, e.DataRozpoczęcia, e.DataZakończenia });
                entity.HasIndex(e => e.UserId);
            });

            // Configure Attachment entity
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).HasMaxLength(200);
                entity.Property(e => e.UploadedByUserId).IsRequired();

                entity.HasOne(a => a.Reservation)
                      .WithMany(r => r.Attachments)
                      .HasForeignKey(a => a.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.ReservationId);
            });
        }
    }
}