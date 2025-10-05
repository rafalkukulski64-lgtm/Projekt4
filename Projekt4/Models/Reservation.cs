using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Projekt4.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        [Required]
        public int SalaId { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Tytuł rezerwacji")]
        public string Tytuł { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime DataRozpoczęcia { get; set; }
        
        [Required]
        [Display(Name = "Data zakończenia")]
        public DateTime DataZakończenia { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Notatki")]
        public string? Notatki { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Data utworzenia")]
        public DateTime DataUtworzenia { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Data aktualizacji")]
        public DateTime? DataAktualizacji { get; set; }
        
        
        [ForeignKey("SalaId")]
        public virtual Room Room { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;
        
        
        [Obsolete("Use Tytuł instead")]
        public string Nazwa 
        { 
            get => Tytuł; 
            set => Tytuł = value; 
        }
        
        [Obsolete("Use DataRozpoczęcia instead")]
        public DateTime Początek 
        { 
            get => DataRozpoczęcia; 
            set => DataRozpoczęcia = value; 
        }
        
        [Obsolete("Use DataZakończenia instead")]
        public DateTime Koniec 
        { 
            get => DataZakończenia; 
            set => DataZakończenia = value; 
        }
        
        [Obsolete("Use Notatki instead")]
        public string? Uwagi 
        { 
            get => Notatki; 
            set => Notatki = value; 
        }
        
        [Obsolete("Use DataUtworzenia instead")]
        public DateTime Data 
        { 
            get => DataUtworzenia; 
            set => DataUtworzenia = value; 
        }
        
        [Obsolete("Use Status instead")]
        public bool Potwierdzona 
        { 
            get => Status == ReservationStatus.Approved; 
            set => Status = value ? ReservationStatus.Approved : ReservationStatus.Pending; 
        }
        
        [Obsolete("Use Status instead")]
        public bool Anulowana 
        { 
            get => Status == ReservationStatus.Cancelled; 
            set => Status = value ? ReservationStatus.Cancelled : ReservationStatus.Pending; 
        }
        
        
        public string Email { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cena { get; set; }
    }
}