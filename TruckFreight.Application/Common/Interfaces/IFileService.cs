using System;
using System.IO;
using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFileAsync(string fileUrl);
        Task DeleteFileAsync(string fileUrl);
        Task<bool> ValidateFileAsync(Stream fileStream, string fileName, string contentType);
    }
} 