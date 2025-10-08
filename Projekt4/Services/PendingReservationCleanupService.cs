using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Projekt4.Data;
using Projekt4.Models;

namespace Projekt4.Services
{
    public class PendingReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ReservationCleanupOptions _options;
        private readonly ILogger<PendingReservationCleanupService> _logger;

        public PendingReservationCleanupService(IServiceProvider serviceProvider, IOptions<ReservationCleanupOptions> options, ILogger<PendingReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CheckIntervalMinutes));
            try
            {
                _logger.LogInformation("PendingReservationCleanupService started. Interval: {interval}.", interval);
            }
            catch { /* Swallow logging provider errors */ }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _logger.LogError(ex, "Error during pending reservations cleanup.");
                    }
                    catch
                    {
                        Console.WriteLine($"Error during pending reservations cleanup: {ex.Message}");
                    }
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            try
            {
                _logger.LogInformation("PendingReservationCleanupService stopped.");
            }
            catch { /* Swallow logging provider errors */ }
        }

        private async Task CleanupAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Prefer seconds threshold if provided; otherwise fallback to days
            DateTime thresholdUtc;
            var seconds = Math.Max(0, _options.SecondsThreshold);
            if (seconds > 0)
            {
                thresholdUtc = DateTime.UtcNow.AddSeconds(-seconds);
            }
            else
            {
                thresholdUtc = DateTime.UtcNow.AddDays(-Math.Max(1, _options.DaysThreshold));
            }

            var toCancel = await db.Reservations
                .Where(r => r.Status == ReservationStatus.Pending && r.DataUtworzenia < thresholdUtc)
                .ToListAsync(ct);

            if (!toCancel.Any())
            {
                try
                {
                    if (seconds > 0)
                        _logger.LogInformation("No pending reservations older than {seconds} seconds.", seconds);
                    else
                        _logger.LogInformation("No pending reservations older than {days} days.", _options.DaysThreshold);
                }
                catch { }
                return;
            }

            foreach (var r in toCancel)
            {
                r.Status = ReservationStatus.Cancelled;
                r.DataAktualizacji = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            try
            {
                if (seconds > 0)
                    _logger.LogInformation("Cancelled {count} pending reservations older than {seconds} seconds.", toCancel.Count, seconds);
                else
                    _logger.LogInformation("Cancelled {count} pending reservations older than {days} days.", toCancel.Count, _options.DaysThreshold);
            }
            catch { }
        }
    }
}