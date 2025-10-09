using System.ComponentModel.DataAnnotations;

namespace Projekt4.Models
{
    public class CreateReservationViewModel
    {
        [Required(ErrorMessage = "Wybór sali jest wymagany")]
        [Display(Name = "Sala")]
        public int SalaId { get; set; }

        [Required(ErrorMessage = "Tytuł rezerwacji jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł nie może być dłuższy niż 200 znaków")]
        [Display(Name = "Tytuł rezerwacji")]
        public string Tytuł { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        [DataType(DataType.DateTime)]
        public DateTime DataRozpoczęcia { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        [DataType(DataType.DateTime)]
        public DateTime DataZakończenia { get; set; } = DateTime.Now.AddHours(2);

        [StringLength(1000, ErrorMessage = "Notatki nie mogą być dłuższe niż 1000 znaków")]
        [Display(Name = "Notatki")]
        [DataType(DataType.MultilineText)]
        public string? Notatki { get; set; }

        public List<Room> AvailableRooms { get; set; } = new List<Room>();

        public Room? SelectedRoom { get; set; }
    }
}