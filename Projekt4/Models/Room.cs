namespace Projekt4.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public int Pojemnosc { get; set; }
        public decimal Cena { get; set; }
        public bool Dostepna { get; set; } = true;
        public DateTime Data { get; set; } = DateTime.UtcNow;

        
        public virtual ICollection<Reservation> Rezerwacje { get; set; } = new List<Reservation>();
    }
}