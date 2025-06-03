using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadDirectory;
        private readonly ILogger<FileService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionIV;
        private readonly IVirusScanService _virusScanService;
        private readonly Dictionary<string, (byte[] Signature, int MaxSize)> _fileSignatures = new Dictionary<string, (byte[], int)>
        {
            { "application/pdf", (new byte[] { 0x25, 0x50, 0x44, 0x46 }, 5 * 1024 * 1024) }, // %PDF, 5MB
            { "image/jpeg", (new byte[] { 0xFF, 0xD8, 0xFF }, 2 * 1024 * 1024) }, // JPEG, 2MB
            { "image/png", (new byte[] { 0x89, 0x50, 0x4E, 0x47 }, 2 * 1024 * 1024) }, // PNG, 2MB
            { "application/msword", (new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }, 5 * 1024 * 1024) }, // DOC, 5MB
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 5 * 1024 * 1024) } // DOCX, 5MB
        };

        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        private readonly string[] _allowedMimeTypes = { 
            "application/pdf",
            "image/jpeg",
            "image/png",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly int _maxFileNameLength = 255;
        private readonly string[] _forbiddenCharacters = { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
        private readonly long _compressionThreshold = 1 * 1024 * 1024; // 1MB
        private readonly string[] _sensitiveFileTypes = { ".pdf", ".doc", ".docx" };

        public FileService(
            IConfiguration configuration, 
            ILogger<FileService> logger,
            IVirusScanService virusScanService)
        {
            _logger = logger;
            _virusScanService = virusScanService;
            _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "uploads";
            
            // Initialize encryption key and IV from configuration
            var key = configuration["FileStorage:EncryptionKey"];
            var iv = configuration["FileStorage:EncryptionIV"];
            
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                throw new InvalidOperationException("Encryption key and IV must be configured.");
            }

            _encryptionKey = Convert.FromBase64String(key);
            _encryptionIV = Convert.FromBase64String(iv);

            try
            {
                if (!Directory.Exists(_uploadDirectory))
                {
                    Directory.CreateDirectory(_uploadDirectory);
                    _logger.LogInformation("Created upload directory: {Directory}", _uploadDirectory);
                }
                // Set directory permissions
                var directoryInfo = new DirectoryInfo(_uploadDirectory);
                directoryInfo.Attributes |= FileAttributes.ReadOnly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create upload directory: {Directory}", _uploadDirectory);
                throw new InvalidOperationException($"Failed to initialize file storage: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                ValidateInput(fileStream, fileName, contentType);

                // Validate file content
                if (!await ValidateFileContentAsync(fileStream, contentType))
                {
                    _logger.LogWarning("File content validation failed for file: {FileName}", fileName);
                    throw new InvalidOperationException("File content validation failed.");
                }

                // Reset stream position after validation
                fileStream.Position = 0;

                // Scan for viruses
                if (!await _virusScanService.ScanFileAsync(fileStream, fileName))
                {
                    _logger.LogWarning("Virus scan failed for file: {FileName}", fileName);
                    throw new InvalidOperationException("File failed virus scan.");
                }

                // Reset stream position after virus scan
                fileStream.Position = 0;

                // Generate a secure filename
                var uniqueFileName = GenerateSecureFileName(fileName);
                var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

                // Process and save file
                await ProcessAndSaveFileAsync(fileStream, filePath, fileName);

                _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    throw new ArgumentException("File URL is required.");
                }

                // Validate file URL for path traversal attempts
                if (fileUrl.Contains("..") || fileUrl.Contains("/") || fileUrl.Contains("\\"))
                {
                    throw new InvalidOperationException("Invalid file URL format.");
                }

                var filePath = Path.Combine(_uploadDirectory, fileUrl);
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    throw new FileNotFoundException("File not found.", fileUrl);
                }

                var memory = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Check if file is encrypted
                    if (IsEncryptedFile(filePath))
                    {
                        await DecryptFileAsync(fileStream, memory);
                    }
                    // Check if file is compressed
                    else if (IsCompressedFile(filePath))
                    {
                        await DecompressFileAsync(fileStream, memory);
                    }
                    else
                    {
                        await fileStream.CopyToAsync(memory);
                    }
                }
                memory.Position = 0;

                _logger.LogInformation("File downloaded successfully: {FilePath}", filePath);
                return memory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    throw new ArgumentException("File URL is required.");
                }

                // Validate file URL for path traversal attempts
                if (fileUrl.Contains("..") || fileUrl.Contains("/") || fileUrl.Contains("\\"))
                {
                    throw new InvalidOperationException("Invalid file URL format.");
                }

                var filePath = Path.Combine(_uploadDirectory, fileUrl);
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    fileInfo.Attributes &= ~FileAttributes.ReadOnly; // Remove read-only attribute
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                throw;
            }
        }

        public async Task<bool> ValidateFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                if (fileStream == null || string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                if (!ValidateFileName(fileName))
                {
                    _logger.LogWarning("Invalid file name: {FileName}", fileName);
                    return false;
                }

                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file extension: {Extension}", extension);
                    return false;
                }

                if (!_allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
                {
                    _logger.LogWarning("Invalid content type: {ContentType}", contentType);
                    return false;
                }

                if (fileStream.Length > _maxFileSize)
                {
                    _logger.LogWarning("File size exceeds limit: {Size}", fileStream.Length);
                    return false;
                }

                return await ValidateFileContentAsync(fileStream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file: {FileName}", fileName);
                return false;
            }
        }

        private void ValidateInput(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File stream and file name are required.");
            }

            if (!ValidateFileName(fileName))
            {
                throw new InvalidOperationException("Invalid file name format.");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            if (!_allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
            {
                throw new InvalidOperationException($"Content type not allowed. Allowed types: {string.Join(", ", _allowedMimeTypes)}");
            }

            if (fileStream.Length > _maxFileSize)
            {
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
            }
        }

        private bool ValidateFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Length > _maxFileNameLength)
            {
                return false;
            }

            foreach (var character in _forbiddenCharacters)
            {
                if (fileName.Contains(character))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateFileContentAsync(Stream fileStream, string contentType)
        {
            try
            {
                if (!_fileSignatures.TryGetValue(contentType.ToLowerInvariant(), out var signatureInfo))
                {
                    return true; // No signature validation for this type
                }

                var (signature, maxSize) = signatureInfo;
                if (fileStream.Length > maxSize)
                {
                    _logger.LogWarning("File size exceeds type-specific limit: {Size} > {MaxSize}", fileStream.Length, maxSize);
                    return false;
                }

                var buffer = new byte[signature.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);
                fileStream.Position = 0;

                for (int i = 0; i < signature.Length; i++)
                {
                    if (buffer[i] != signature[i])
                    {
                        _logger.LogWarning("File signature validation failed for content type: {ContentType}", contentType);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file content for type: {ContentType}", contentType);
                return false;
            }
        }

        private async Task ProcessAndSaveFileAsync(Stream sourceStream, string filePath, string originalFileName)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Check if file should be encrypted
                    if (ShouldEncryptFile(originalFileName))
                    {
                        await EncryptFileAsync(sourceStream, fileStream);
                    }
                    // Check if file should be compressed
                    else if (sourceStream.Length > _compressionThreshold)
                    {
                        await CompressFileAsync(sourceStream, fileStream);
                    }
                    else
                    {
                        await sourceStream.CopyToAsync(fileStream);
                    }
                }

                var fileInfo = new FileInfo(filePath);
                fileInfo.Attributes |= FileAttributes.ReadOnly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing and saving file: {FilePath}", filePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                throw;
            }
        }

        private bool ShouldEncryptFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return Array.Exists(_sensitiveFileTypes, x => x == extension);
        }

        private async Task EncryptFileAsync(Stream sourceStream, Stream destinationStream)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = _encryptionIV;

                using (var cryptoStream = new CryptoStream(destinationStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    await sourceStream.CopyToAsync(cryptoStream);
                }
            }
        }

        private async Task DecryptFileAsync(Stream sourceStream, Stream destinationStream)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = _encryptionIV;

                using (var cryptoStream = new CryptoStream(sourceStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    await cryptoStream.CopyToAsync(destinationStream);
                }
            }
        }

        private async Task CompressFileAsync(Stream sourceStream, Stream destinationStream)
        {
            using (var gzipStream = new GZipStream(destinationStream, CompressionLevel.Optimal))
            {
                await sourceStream.CopyToAsync(gzipStream);
            }
        }

        private async Task DecompressFileAsync(Stream sourceStream, Stream destinationStream)
        {
            using (var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(destinationStream);
            }
        }

        private bool IsEncryptedFile(string filePath)
        {
            // Check file extension or header for encryption marker
            return Path.GetExtension(filePath).EndsWith(".enc");
        }

        private bool IsCompressedFile(string filePath)
        {
            // Check file extension or header for compression marker
            return Path.GetExtension(filePath).EndsWith(".gz");
        }

        private string GenerateSecureFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.Ticks;
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var randomString = BitConverter.ToString(randomBytes).Replace("-", "").ToLowerInvariant();
            return $"{timestamp}-{randomString}{extension}";
        }
    }
} 