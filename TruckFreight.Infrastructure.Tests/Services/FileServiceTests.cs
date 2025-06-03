using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using TruckFreight.Infrastructure.Services;

namespace TruckFreight.Infrastructure.Tests.Services
{
    public class FileServiceTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly FileService _fileService;
        private readonly IConfiguration _configuration;

        public FileServiceTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "TruckFreightTests");
            Directory.CreateDirectory(_testDirectory);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FileStorage:UploadDirectory", _testDirectory),
                    new KeyValuePair<string, string>("FileStorage:MaxFileSize", "10485760"),
                })
                .Build();

            _configuration = configuration;
            _fileService = new FileService(_configuration);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_ReturnsFileName()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.pdf";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileService.UploadFileAsync(stream, fileName, "application/pdf");

            // Assert
            Assert.NotNull(result);
            Assert.EndsWith(".pdf", result);
            Assert.True(File.Exists(Path.Combine(_testDirectory, result)));
        }

        [Fact]
        public async Task UploadFileAsync_InvalidExtension_ThrowsException()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.exe";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _fileService.UploadFileAsync(stream, fileName, "application/octet-stream"));
        }

        [Fact]
        public async Task DownloadFileAsync_ExistingFile_ReturnsStream()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.pdf";
            var filePath = Path.Combine(_testDirectory, fileName);
            await File.WriteAllTextAsync(filePath, content);

            // Act
            using var result = await _fileService.DownloadFileAsync(fileName);

            // Assert
            Assert.NotNull(result);
            using var reader = new StreamReader(result);
            var downloadedContent = await reader.ReadToEndAsync();
            Assert.Equal(content, downloadedContent);
        }

        [Fact]
        public async Task DownloadFileAsync_NonExistingFile_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _fileService.DownloadFileAsync("nonexistent.pdf"));
        }

        [Fact]
        public async Task DeleteFileAsync_ExistingFile_DeletesFile()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.pdf";
            var filePath = Path.Combine(_testDirectory, fileName);
            await File.WriteAllTextAsync(filePath, content);

            // Act
            await _fileService.DeleteFileAsync(fileName);

            // Assert
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public async Task ValidateFileAsync_ValidFile_ReturnsTrue()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.pdf";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileService.ValidateFileAsync(stream, fileName, "application/pdf");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateFileAsync_InvalidExtension_ReturnsFalse()
        {
            // Arrange
            var content = "Test content";
            var fileName = "test.exe";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileService.ValidateFileAsync(stream, fileName, "application/octet-stream");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateFileAsync_TooLargeFile_ReturnsFalse()
        {
            // Arrange
            var content = new byte[11 * 1024 * 1024]; // 11MB
            var fileName = "test.pdf";
            using var stream = new MemoryStream(content);

            // Act
            var result = await _fileService.ValidateFileAsync(stream, fileName, "application/pdf");

            // Assert
            Assert.False(result);
        }
    }
} 