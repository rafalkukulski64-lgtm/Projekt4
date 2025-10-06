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

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel viewModel)
        {
            
            viewModel.AvailableRooms = await _context.Rooms
                .Where(r => r.CzyAktywna)
                .OrderBy(r => r.Nazwa)
                .ToListAsync();

            
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

            
            if (!isManager && reservation.UserId != userId)
            {
                return Forbid();
            }

            return View(reservation);
        }

        
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AllReservations(int? roomId, DateTime? startDate, DateTime? endDate, ReservationStatus? status, string? title)
        {
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .AsQueryable();

            
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

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(r => r.Tytuł.Contains(title));
            }

            var reservations = await query
                .OrderByDescending(r => r.DataUtworzenia)
                .ToListAsync();

            
            ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Nazwa).ToListAsync(), "Id", "Nazwa", roomId);
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ReservationStatus)).Cast<ReservationStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) }), "Value", "Text", (int?)status);
            ViewBag.CurrentRoomId = roomId;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentTitle = title;

            return View(reservations);
        }

        
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

            
            if ((status == ReservationStatus.Approved || status == ReservationStatus.Rejected) && reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = "Tę rezerwację można zatwierdzić/odrzucić tylko w statusie Oczekująca.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (status == ReservationStatus.Approved)
            {
                
                await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                var hasConflict = await _context.Reservations
                    .Where(r => r.SalaId == reservation.SalaId && r.Id != reservation.Id &&
                                r.Status != ReservationStatus.Rejected &&
                                r.Status != ReservationStatus.Cancelled &&
                                (r.DataRozpoczęcia < reservation.DataZakończenia && r.DataZakończenia > reservation.DataRozpoczęcia))
                    .AnyAsync();

                if (hasConflict)
                {
                    TempData["ErrorMessage"] = "Zatwierdzenie nie jest możliwe – wykryto kolizję terminu.";
                    await tx.RollbackAsync();
                    return RedirectToAction(nameof(Details), new { id });
                }

                reservation.Status = ReservationStatus.Approved;
                reservation.DataAktualizacji = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            else if (status == ReservationStatus.Rejected)
            {
                reservation.Status = ReservationStatus.Rejected;
                reservation.DataAktualizacji = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                
                reservation.Status = status;
                reservation.DataAktualizacji = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Status rezerwacji został zmieniony na: {GetStatusDisplayName(status)}";
            return RedirectToAction(nameof(Details), new { id });
        }

        
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

            
            if (!isManager && reservation.UserId != userId)
            {
                return Forbid();
            }

            
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

        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isManager = User.IsInRole("Manager");

            
            if (!isManager && reservation.UserId != userId)
            {
                return Forbid();
            }

            
            if (reservation.Status != ReservationStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Usuwanie możliwe tylko dla rezerwacji w statusie Anulowana.";
                if (isManager)
                {
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    return RedirectToAction(nameof(MyReservations));
                }
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rezerwacja została usunięta.";
            if (isManager)
            {
                return RedirectToAction(nameof(AllReservations));
            }
            else
            {
                return RedirectToAction(nameof(MyReservations));
            }
        }
    }
}