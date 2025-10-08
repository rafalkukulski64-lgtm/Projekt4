namespace Projekt4.Services
{
    public class ReservationCleanupOptions
    {
        // Prefer SecondsThreshold when provided (>0); fallback to DaysThreshold
        public int DaysThreshold { get; set; } = 3;
        public int SecondsThreshold { get; set; } = 0;
        // Reminder threshold in seconds, independent from cleanup thresholds
        public int ReminderSecondsThreshold { get; set; } = 0;
        public int CheckIntervalMinutes { get; set; } = 60;
    }
}