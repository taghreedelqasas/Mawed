using Microsoft.AspNetCore.Http;

namespace Maw3ed.APIs.Helpers
{
    public static class FileStorageHelper
    {
        public static async Task<string> SavePdfAsync(
            IFormFile file,
            string webRootPath)
        {
            var reportsFolder = Path.Combine(webRootPath, "MedicalReports");

            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            var uniqueFileName =
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var fullPath = Path.Combine(reportsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueFileName;
        }
    }
}