using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Infrastructure.Services
{
    public class FileBackupService : IFileBackupService
    {
        private readonly string _backupDirectory;
        private readonly ILogger<FileBackupService> _logger;
        private readonly IFileService _fileService;
        private readonly Dictionary<string, string> _backupTypes = new Dictionary<string, string>
        {
            { "daily", "yyyy-MM-dd" },
            { "weekly", "yyyy-'W'ww" },
            { "monthly", "yyyy-MM" }
        };

        public FileBackupService(
            IConfiguration configuration,
            ILogger<FileBackupService> logger,
            IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
            _backupDirectory = configuration["FileStorage:BackupDirectory"] ?? "backups";

            try
            {
                if (!Directory.Exists(_backupDirectory))
                {
                    Directory.CreateDirectory(_backupDirectory);
                    _logger.LogInformation("Created backup directory: {Directory}", _backupDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup directory: {Directory}", _backupDirectory);
                throw new InvalidOperationException($"Failed to initialize backup service: {ex.Message}", ex);
            }
        }

        public async Task<string> CreateBackupAsync(string filePath, string backupType)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("File not found for backup", filePath);
                }

                if (!_backupTypes.ContainsKey(backupType))
                {
                    throw new ArgumentException($"Invalid backup type. Allowed types: {string.Join(", ", _backupTypes.Keys)}");
                }

                var timestamp = DateTime.UtcNow.ToString(_backupTypes[backupType]);
                var backupId = GenerateBackupId(filePath, timestamp);
                var backupPath = Path.Combine(_backupDirectory, backupId);

                // Create backup directory if it doesn't exist
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Copy file to backup location
                using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                // Calculate file hash
                var hash = await CalculateFileHashAsync(backupPath);

                // Create backup info file
                var backupInfo = new BackupInfo
                {
                    BackupId = backupId,
                    OriginalPath = filePath,
                    BackupPath = backupPath,
                    BackupType = backupType,
                    CreatedAt = DateTime.UtcNow,
                    Size = new FileInfo(backupPath).Length,
                    Hash = hash
                };

                await SaveBackupInfoAsync(backupInfo);

                _logger.LogInformation("Created backup {BackupId} for file {FilePath}", backupId, filePath);
                return backupId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup for file {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupId, string targetPath)
        {
            try
            {
                var backupInfo = await GetBackupInfoAsync(backupId);
                if (backupInfo == null)
                {
                    throw new FileNotFoundException("Backup not found", backupId);
                }

                if (!File.Exists(backupInfo.BackupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupInfo.BackupPath);
                }

                // Verify backup integrity
                var currentHash = await CalculateFileHashAsync(backupInfo.BackupPath);
                if (currentHash != backupInfo.Hash)
                {
                    throw new InvalidOperationException("Backup file integrity check failed");
                }

                // Create target directory if it doesn't exist
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Copy backup to target location
                using (var sourceStream = new FileStream(backupInfo.BackupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                _logger.LogInformation("Restored backup {BackupId} to {TargetPath}", backupId, targetPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup {BackupId}", backupId);
                return false;
            }
        }

        public async Task<IEnumerable<BackupInfo>> GetBackupsAsync(string filePath)
        {
            try
            {
                var backups = new List<BackupInfo>();
                var backupDir = Path.Combine(_backupDirectory, "info");
                
                if (!Directory.Exists(backupDir))
                {
                    return backups;
                }

                var infoFiles = Directory.GetFiles(backupDir, "*.json");
                foreach (var infoFile in infoFiles)
                {
                    var backupInfo = await LoadBackupInfoAsync(infoFile);
                    if (backupInfo.OriginalPath == filePath)
                    {
                        backups.Add(backupInfo);
                    }
                }

                return backups.OrderByDescending(b => b.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backups for file {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteBackupAsync(string backupId)
        {
            try
            {
                var backupInfo = await GetBackupInfoAsync(backupId);
                if (backupInfo == null)
                {
                    return false;
                }

                if (File.Exists(backupInfo.BackupPath))
                {
                    File.Delete(backupInfo.BackupPath);
                }

                var infoPath = Path.Combine(_backupDirectory, "info", $"{backupId}.json");
                if (File.Exists(infoPath))
                {
                    File.Delete(infoPath);
                }

                _logger.LogInformation("Deleted backup {BackupId}", backupId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup {BackupId}", backupId);
                return false;
            }
        }

        public async Task<bool> CleanupOldBackupsAsync(TimeSpan retentionPeriod)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(retentionPeriod);
                var backupDir = Path.Combine(_backupDirectory, "info");
                
                if (!Directory.Exists(backupDir))
                {
                    return true;
                }

                var infoFiles = Directory.GetFiles(backupDir, "*.json");
                foreach (var infoFile in infoFiles)
                {
                    var backupInfo = await LoadBackupInfoAsync(infoFile);
                    if (backupInfo.CreatedAt < cutoffDate)
                    {
                        await DeleteBackupAsync(backupInfo.BackupId);
                    }
                }

                _logger.LogInformation("Cleaned up backups older than {CutoffDate}", cutoffDate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old backups");
                return false;
            }
        }

        private string GenerateBackupId(string filePath, string timestamp)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var hash = CalculateStringHash($"{filePath}{timestamp}");
            return $"{fileName}_{timestamp}_{hash}{extension}";
        }

        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string CalculateStringHash(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant().Substring(0, 8);
        }

        private async Task SaveBackupInfoAsync(BackupInfo backupInfo)
        {
            var infoDir = Path.Combine(_backupDirectory, "info");
            if (!Directory.Exists(infoDir))
            {
                Directory.CreateDirectory(infoDir);
            }

            var infoPath = Path.Combine(infoDir, $"{backupInfo.BackupId}.json");
            var json = System.Text.Json.JsonSerializer.Serialize(backupInfo);
            await File.WriteAllTextAsync(infoPath, json);
        }

        private async Task<BackupInfo> LoadBackupInfoAsync(string infoPath)
        {
            var json = await File.ReadAllTextAsync(infoPath);
            return System.Text.Json.JsonSerializer.Deserialize<BackupInfo>(json);
        }

        private async Task<BackupInfo> GetBackupInfoAsync(string backupId)
        {
            var infoPath = Path.Combine(_backupDirectory, "info", $"{backupId}.json");
            if (!File.Exists(infoPath))
            {
                return null;
            }

            return await LoadBackupInfoAsync(infoPath);
        }
    }
} 