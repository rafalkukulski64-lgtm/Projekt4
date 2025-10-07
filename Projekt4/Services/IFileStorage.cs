using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Projekt4.Services
{
    public interface IFileStorage
    {
        Task<(string storedFileName, long size)> SaveReservationAttachmentAsync(int reservationId, IFormFile file);
        string GetReservationAttachmentPath(int reservationId, string storedFileName);
        bool DeleteReservationAttachment(int reservationId, string storedFileName);
    }
}