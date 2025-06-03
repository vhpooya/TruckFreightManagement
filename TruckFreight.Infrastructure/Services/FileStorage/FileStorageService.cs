using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Infrastructure.Services.FileStorage
{
   public class FileStorageService : IFileStorageService
   {
       private readonly IConfiguration _configuration;
       private readonly ILogger<FileStorageService> _logger;
       private readonly string _basePath;
       private readonly long _maxFileSize;
       private readonly List<string> _allowedExtensions;

       public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
       {
           _configuration = configuration;
           _logger = logger;
           _basePath = _configuration["FileStorage:BasePath"] ?? "uploads";
           _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 5242880); // 5MB default
           _allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<List<string>>() 
               ?? new List<string> { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };

           // Ensure upload directory exists
           if (!Directory.Exists(_basePath))
           {
               Directory.CreateDirectory(_basePath);
           }
       }

       public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder = "general")
       {
           try
           {
               if (file == null || file.Length == 0)
               {
                   return new FileUploadResult
                   {
                       IsSuccess = false,
                       ErrorMessage = "فایل انتخاب نشده است"
                   };
               }

               // Validate file size
               if (file.Length > _maxFileSize)
               {
                   return new FileUploadResult
                   {
                       IsSuccess = false,
                       ErrorMessage = $"حجم فایل نباید بیش از {_maxFileSize / 1024 / 1024} مگابایت باشد"
                   };
               }

               // Validate file extension
               var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
               if (!_allowedExtensions.Contains(extension))
               {
                   return new FileUploadResult
                   {
                       IsSuccess = false,
                       ErrorMessage = $"فرمت فایل مجاز نیست. فرمت‌های مجاز: {string.Join(", ", _allowedExtensions)}"
                   };
               }

               // Create folder structure
               var folderPath = Path.Combine(_basePath, folder, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
               if (!Directory.Exists(folderPath))
               {
                   Directory.CreateDirectory(folderPath);
               }

               // Generate unique filename
               var fileName = $"{Guid.NewGuid()}{extension}";
               var filePath = Path.Combine(folderPath, fileName);

               // Save file
               using (var stream = new FileStream(filePath, FileMode.Create))
               {
                   await file.CopyToAsync(stream);
               }

               var relativePath = Path.GetRelativePath(_basePath, filePath).Replace('\\', '/');

               _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);

               return new FileUploadResult
               {
                   IsSuccess = true,
                   FileName = file.FileName,
                   FilePath = relativePath,
                   FileSize = file.Length,
                   ContentType = file.ContentType
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
               return new FileUploadResult
               {
                   IsSuccess = false,
                   ErrorMessage = "خطا در آپلود فایل"
               };
           }
       }

       public async Task<FileDownloadResult> DownloadFileAsync(string filePath)
       {
           try
           {
               var fullPath = Path.Combine(_basePath, filePath);

               if (!File.Exists(fullPath))
               {
                   return new FileDownloadResult
                   {
                       IsSuccess = false,
                       ErrorMessage = "فایل یافت نشد"
                   };
               }

               var content = await File.ReadAllBytesAsync(fullPath);
               var fileName = Path.GetFileName(fullPath);
               var contentType = GetContentType(Path.GetExtension(fullPath));

               return new FileDownloadResult
               {
                   IsSuccess = true,
                   Content = content,
                   FileName = fileName,
                   ContentType = contentType
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
               return new FileDownloadResult
               {
                   IsSuccess = false,
                   ErrorMessage = "خطا در دانلود فایل"
               };
           }
       }

       public async Task<bool> DeleteFileAsync(string filePath)
       {
           try
           {
               var fullPath = Path.Combine(_basePath, filePath);

               if (File.Exists(fullPath))
               {
                   File.Delete(fullPath);
                   _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                   return true;
               }

               return false;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
               return false;
           }
       }

       public bool FileExists(string filePath)
       {
           var fullPath = Path.Combine(_basePath, filePath);
           return File.Exists(fullPath);
       }

       public long GetFileSize(string filePath)
       {
           var fullPath = Path.Combine(_basePath, filePath);
           return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
       }

       private string GetContentType(string extension)
       {
           return extension.ToLowerInvariant() switch
           {
               ".jpg" or ".jpeg" => "image/jpeg",
               ".png" => "image/png",
               ".gif" => "image/gif",
               ".pdf" => "application/pdf",
               ".doc" => "application/msword",
               ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
               ".xls" => "application/vnd.ms-excel",
               ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               ".txt" => "text/plain",
               ".zip" => "application/zip",
               _ => "application/octet-stream"
           };
       }
   }

   // File service interfaces and models
   public interface IFileStorageService
   {
       Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder = "general");
       Task<FileDownloadResult> DownloadFileAsync(string filePath);
       Task<bool> DeleteFileAsync(string filePath);
       bool FileExists(string filePath);
       long GetFileSize(string filePath);
   }

   public class FileUploadResult
   {
       public bool IsSuccess { get; set; }
       public string FileName { get; set; }
       public string FilePath { get; set; }
       public long FileSize { get; set; }
       public string ContentType { get; set; }
       public string ErrorMessage { get; set;}
	   
   }
	   
	   