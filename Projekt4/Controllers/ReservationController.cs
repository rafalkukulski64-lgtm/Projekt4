using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt4.Data;
using Projekt4.Models;

namespace Projekt4.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservation/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateReservationViewModel
            {
                AvailableRooms = await _context.Rooms
                    .Where(r => r.CzyAktywna)
                    .OrderBy(r => r.Nazwa)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel viewModel)
        {
            // Reload available rooms for the view
            viewModel.AvailableRooms = await _context.Rooms
                .Where(r => r.CzyAktywna)
                .OrderBy(r => r.Nazwa)
                .ToListAsync();

            // Custom validation
            if (viewModel.DataRozpoczęcia >= viewModel.DataZakończenia)
            {
                ModelState.AddModelError("DataZakończenia", "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
            }

            if (viewModel.DataRozpoczęcia < DateTime.Now)
            {
                ModelState.AddModelError("DataRozpoczęcia", "Nie można tworzyć rezerwacji w przeszłości.");
            }

            if (ModelState.IsValid)
            {
                // Check for conflicts
                var hasConflict = await _context.Reservations
                    .Where(r => r.SalaId == viewModel.SalaId &&
                               r.Status != ReservationStatus.Rejected &&
                               r.Status != ReservationStatus.Cancelled &&
                               ((r.DataRozpoczęcia < viewModel.DataZakończenia && r.DataZakończenia > viewModel.DataRozpoczęcia)))
                    .AnyAsync();

                if (hasConflict)
                {
                    ModelState.AddModelError("", "W wybranym terminie sala jest już zarezerwowana. Wybierz inny termin.");
                    return View(viewModel);
                }

                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Challenge();
                }

                var reservation = new Reservation
                {
                    SalaId = viewModel.SalaId,
                    Tytuł = viewModel.Tytuł,
                    DataRozpoczęcia = viewModel.DataRozpoczęcia,
                    DataZakończenia = viewModel.DataZakończenia,
                    Notatki = viewModel.Notatki,
                    UserId = userId,
                    Status = ReservationStatus.Pending,
                    DataUtworzenia = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Rezerwacja została złożona pomyślnie i oczekuje na zatwierdzenie.";
                return RedirectToAction(nameof(MyReservations));
            }

            return View(viewModel);
        }

        // GET: Reservation/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.DataUtworzenia)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isManager = User.IsInRole("Manager");

            // Check if user can view this reservation
            if (!isManager && reservation.UserId != userId)
            {
                return Forbid();
            }

            return View(reservation);
        }

        // GET: Reservation/AllReservations (Manager only)
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AllReservations(int? roomId, DateTime? startDate, DateTime? endDate, ReservationStatus? status)
        {
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .AsQueryable();

            // Apply filters
            if (roomId.HasValue)
            {
                query = query.Where(r => r.SalaId == roomId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.DataRozpoczęcia >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.DataZakończenia <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var reservations = await query
                .OrderByDescending(r => r.DataUtworzenia)
                .ToListAsync();

            // Prepare filter data for the view
            ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Nazwa).ToListAsync(), "Id", "Nazwa", roomId);
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ReservationStatus)).Cast<ReservationStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) }), "Value", "Text", (int?)status);
            ViewBag.CurrentRoomId = roomId;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status;

            return View(reservations);
        }

        // POST: Reservation/UpdateStatus/5 (Manager only)
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Status rezerwacji został zmieniony na: {GetStatusDisplayName(status)}";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Reservation/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isManager = User.IsInRole("Manager");

            // Check if user can cancel this reservation
            if (!isManager && reservation.UserId != userId)
            {
                return Forbid();
            }

            // Only allow cancellation of pending or approved reservations
            if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Approved)
            {
                TempData["ErrorMessage"] = "Nie można anulować tej rezerwacji.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(MyReservations));
        }

        // Helper method to get display name for status
        private string GetStatusDisplayName(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Pending => "Oczekująca",
                ReservationStatus.Approved => "Zatwierdzona",
                ReservationStatus.Rejected => "Odrzucona",
                ReservationStatus.Cancelled => "Anulowana",
                _ => status.ToString()
            };
        }

        // API endpoint to check room availability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int roomId, DateTime startDate, DateTime endDate)
        {
            var hasConflict = await _context.Reservations
                .Where(r => r.SalaId == roomId &&
                           r.Status != ReservationStatus.Rejected &&
                           r.Status != ReservationStatus.Cancelled &&
                           ((r.DataRozpoczęcia < endDate && r.DataZakończenia > startDate)))
                .AnyAsync();

            return Json(new { available = !hasConflict });
        }
    }
}