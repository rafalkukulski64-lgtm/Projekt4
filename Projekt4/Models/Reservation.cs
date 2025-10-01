using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt4.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int SalaId { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public DateTime Początek { get; set; }
        public DateTime Koniec { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cena { get; set; }
        
        public string? Uwagi { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public bool Potwierdzona { get; set; } = false;
        public bool Anulowana { get; set; } = false;

        
        [ForeignKey("SalaId")]
        public virtual Room Room { get; set; } = null!;
    }
}