namespace TruckFreight.WebAPI.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string> GetFileUrlAsync(string fileName);
    }
} 