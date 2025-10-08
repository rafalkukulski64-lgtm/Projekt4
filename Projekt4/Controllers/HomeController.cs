using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Projekt4.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Projekt4.Services;

namespace Projekt4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Projekt4.Data.ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;
        private readonly ReservationCleanupOptions _cleanupOptions;

        public HomeController(ILogger<HomeController> logger, Projekt4.Data.ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager, IOptions<ReservationCleanupOptions> cleanupOptions)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _cleanupOptions = cleanupOptions.Value;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index()
        {
            
            var userId = _userManager.GetUserId(User);
            int daysThreshold = Math.Max(1, _cleanupOptions.DaysThreshold);
            
            int secondsThreshold = Math.Max(0, _cleanupOptions.ReminderSecondsThreshold > 0 ? _cleanupOptions.ReminderSecondsThreshold : _cleanupOptions.SecondsThreshold);
            int pendingCount = 0;
            bool isManager = User.IsInRole("Manager");
            if (isManager)
            {
                DateTime thresholdUtc;
                if (secondsThreshold > 0)
                {
                    thresholdUtc = DateTime.UtcNow.AddSeconds(-secondsThreshold);
                }
                else
                {
                    thresholdUtc = DateTime.UtcNow.AddDays(-daysThreshold);
                }
                
                pendingCount = await _context.Reservations
                    .Where(r => r.Status == Projekt4.Models.ReservationStatus.Pending && r.DataUtworzenia < thresholdUtc)
                    .CountAsync();
            }

            ViewBag.PendingReminderDays = daysThreshold;
            ViewBag.PendingReminderSeconds = secondsThreshold;
            ViewBag.PendingReminderCount = pendingCount;
            ViewBag.IsManager = isManager;
            
            ViewBag.CleanupThresholdSeconds = Math.Max(0, _cleanupOptions.SecondsThreshold);
            ViewBag.CleanupThresholdDays = Math.Max(1, _cleanupOptions.DaysThreshold);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
