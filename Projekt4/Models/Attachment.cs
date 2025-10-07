using System;
using System.ComponentModel.DataAnnotations;

namespace Projekt4.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;

        public virtual Reservation Reservation { get; set; } = null!;
    }
}