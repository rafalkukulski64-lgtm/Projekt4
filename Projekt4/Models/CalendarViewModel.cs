using System;
using System.Collections.Generic;

namespace Projekt4.Models
{
    public class CalendarViewModel
    {
        public Room Room { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ViewMode { get; set; } = "week"; // "week" or "month"
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}