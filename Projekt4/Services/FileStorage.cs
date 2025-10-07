using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Projekt4.Services
{
    public class FileStorage : IFileStorage
    {
        private readonly string _basePath;

        public FileStorage(IWebHostEnvironment env)
        {
            _basePath = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "reservations");
            Directory.CreateDirectory(_basePath);
        }

        public async Task<(string storedFileName, long size)> SaveReservationAttachmentAsync(int reservationId, IFormFile file)
        {
            var reservationDir = Path.Combine(_basePath, reservationId.ToString());
            Directory.CreateDirectory(reservationDir);

            var uniqueName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(reservationDir, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (uniqueName, file.Length);
        }

        public string GetReservationAttachmentPath(int reservationId, string storedFileName)
        {
            return Path.Combine(_basePath, reservationId.ToString(), storedFileName);
        }

        public bool DeleteReservationAttachment(int reservationId, string storedFileName)
        {
            var path = GetReservationAttachmentPath(reservationId, storedFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
    }
}