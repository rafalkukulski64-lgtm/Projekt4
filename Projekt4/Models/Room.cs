using System.ComponentModel.DataAnnotations;

namespace Projekt4.Models
{
    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nazwa { get; set; } = string.Empty;
        
        [Required]
        public int Pojemność { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Lokalizacja { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Wyposażenie { get; set; }
        
        public bool CzyAktywna { get; set; } = true;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}