using System;
using System.IO;
using System.Threading.Tasks;
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
                _logger.Warn($", ")
            }

            var backupPath = GetBackupPath(subtitlePath);

            if (_fileSystem.FileExists(backupPath))
            {
                _logger.Debug($", ")
                return false;
            }

            try
            {
                await _fileSystem.CopyFileAsync(subtitlePath, backupPath);
                _logger.Info($", ")
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $