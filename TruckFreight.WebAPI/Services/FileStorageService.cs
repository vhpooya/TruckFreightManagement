using Microsoft.AspNetCore.Hosting;

namespace TruckFreight.WebAPI.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IWebHostEnvironment environment,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return null;
                }

                // Create folder if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return false;
                }

                var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file");
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return null;
                }

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                if (File.Exists(filePath))
                {
                    return $"/uploads/{fileName}";
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting file URL");
                return null;
            }
        }
    }
} 