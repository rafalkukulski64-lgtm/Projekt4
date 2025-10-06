using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt4.Data;
using Projekt4.Models;

namespace Projekt4.Controllers
{
    [Authorize(Roles = "Manager")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Room/Calendar/5
        public async Task<IActionResult> Calendar(int? id, string? mode, DateTime? start)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var viewMode = string.IsNullOrWhiteSpace(mode) ? "week" : mode.ToLowerInvariant();

            DateTime baseDate = start?.Date ?? DateTime.Today;
            DateTime rangeStart;
            DateTime rangeEnd;

            if (viewMode == "month")
            {
                rangeStart = new DateTime(baseDate.Year, baseDate.Month, 1);
                rangeEnd = rangeStart.AddMonths(1).AddTicks(-1);
            }
            else
            {
                int diff = (7 + (baseDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                rangeStart = baseDate.AddDays(-diff).Date;
                rangeEnd = rangeStart.AddDays(7).AddTicks(-1);
                viewMode = "week";
            }

            var reservations = await _context.Reservations
                .Where(r => r.SalaId == id &&
                            r.Status != ReservationStatus.Rejected &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.DataRozpoczęcia < rangeEnd && r.DataZakończenia > rangeStart)
                .OrderBy(r => r.DataRozpoczęcia)
                .ToListAsync();

            var vm = new Projekt4.Models.CalendarViewModel
            {
                Room = room,
                StartDate = rangeStart,
                EndDate = rangeEnd,
                ViewMode = viewMode,
                Reservations = reservations
            };

            ViewBag.BaseDate = baseDate.ToString("yyyy-MM-dd");
            return View(vm);
        }

        // GET: Room/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nazwa,Pojemność,Lokalizacja,Wyposażenie,CzyAktywna")] Room room)
        {
            if (ModelState.IsValid)
            {
                // Check for unique name within location
                var existingRoom = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.Nazwa == room.Nazwa && r.Lokalizacja == room.Lokalizacja);
                
                if (existingRoom != null)
                {
                    ModelState.AddModelError("Nazwa", "Sala o tej nazwie już istnieje w tej lokalizacji.");
                    return View(room);
                }

                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Room/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazwa,Pojemność,Lokalizacja,Wyposażenie,CzyAktywna")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms
                        .FirstOrDefaultAsync(r => r.Nazwa == room.Nazwa && r.Lokalizacja == room.Lokalizacja && r.Id != room.Id);
                    
                    if (existingRoom != null)
                    {
                        ModelState.AddModelError("Nazwa", "Sala o tej nazwie już istnieje w tej lokalizacji.");
                        return View(room);
                    }

                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Room/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}