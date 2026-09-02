using System;
using System.IO;
using System.Threading.Tasks;
using SubtitleSync.Shared.Interfaces;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Manages backup and restore operations for subtitle files.
    /// </summary>
    public class BackupManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly string _backupSuffix;

        /// <summary>
        /// Initializes a new instance of the BackupManager class.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="backupSuffix">The suffix to use for backup files (default: ".syncbackup").</param>
        public BackupManager(IFileSystem fileSystem, ILogger logger, string backupSuffix = ".syncbackup")
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backupSuffix = backupSuffix ?? throw new ArgumentNullException(nameof(backupSuffix));
        }

        /// <summary>
        /// Creates a backup of a subtitle file.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if backup was created, false if it already exists.</returns>
        public async Task<bool> CreateBackupAsync(string subtitlePath)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                throw new ArgumentException("Subtitle path cannot be null or empty.", nameof(subtitlePath));

            if (!_fileSystem.FileExists(subtitlePath))
            {
                _logger.Warn("Subtitle file does not exist: {SubtitlePath}", subtitlePath);
                return false;
            }

            var backupPath = GetBackupPath(subtitlePath);

            if (_fileSystem.FileExists(backupPath))
            {
                _logger.Debug("Backup already exists: {BackupPath}", backupPath);
                return false;
            }

            try
            {
                await _fileSystem.CopyFileAsync(subtitlePath, backupPath);
                _logger.Info("Created backup: {BackupPath}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create backup for: {SubtitlePath}");
                return false;
            }
        }

        /// <summary>
        /// Restores a subtitle file from its backup.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if restore was successful, false otherwise.</returns>
        public async Task<bool> RestoreBackupAsync(string subtitlePath)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                throw new ArgumentException("Subtitle path cannot be null or empty.", nameof(subtitlePath));

            var backupPath = GetBackupPath(subtitlePath);

            if (!_fileSystem.FileExists(backupPath))
            {
                _logger.Warn("Backup file does not exist: {BackupPath}", backupPath);
                return false;
            }

            try
            {
                await _fileSystem.CopyFileAsync(backupPath, subtitlePath, true);
                _logger.Info("Restored from backup: {BackupPath}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to restore backup for: {SubtitlePath}");
                return false;
            }
        }

        /// <summary>
        /// Gets the backup path for a given subtitle file path.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>The backup file path.</returns>
        public string GetBackupPath(string subtitlePath)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                throw new ArgumentException("Subtitle path cannot be null or empty.", nameof(subtitlePath));

            return subtitlePath + _backupSuffix;
        }

        /// <summary>
        /// Checks if a backup exists for the given subtitle file.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if backup exists, false otherwise.</returns>
        public bool BackupExists(string subtitlePath)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                return false;

            return _fileSystem.FileExists(GetBackupPath(subtitlePath));
        }

        /// <summary>
        /// Deletes the backup file for a given subtitle file.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteBackupAsync(string subtitlePath)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                return false;

            var backupPath = GetBackupPath(subtitlePath);

            if (!_fileSystem.FileExists(backupPath))
                return false;

            try
            {
                await _fileSystem.DeleteFileAsync(backupPath);
                _logger.Info("Deleted backup: {BackupPath}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete backup: {BackupPath}");
                return false;
            }
        }
    }
}
