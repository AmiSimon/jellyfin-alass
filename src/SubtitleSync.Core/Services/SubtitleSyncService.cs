using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SubtitleSync.Shared.Interfaces;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;
using SubtitleSync.Core.SubtitleParsers;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Main service for synchronizing subtitles.
    /// </summary>
    public class SubtitleSyncService
    {
        private readonly BackupManager _backupManager;
        private readonly SyncDetector _syncDetector;
        private readonly SyncCorrector _syncCorrector;
        private readonly ILogger _logger;
        private readonly double _minConfidenceThreshold;
        private readonly bool _createBackups;

        /// <summary>
        /// Initializes a new instance of the SubtitleSyncService class.
        /// </summary>
        /// <param name="backupManager">The backup manager.</param>
        /// <param name="syncDetector">The sync detector.</param>
        /// <param name="syncCorrector">The sync corrector.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="minConfidenceThreshold">Minimum confidence threshold for auto-correction.</param>
        /// <param name="createBackups">Whether to create backups before modifying files.</param>
        public SubtitleSyncService(
            BackupManager backupManager,
            SyncDetector syncDetector,
            SyncCorrector syncCorrector,
            ILogger logger,
            double minConfidenceThreshold = 0.9,
            bool createBackups = true)
        {
            _backupManager = backupManager ?? throw new ArgumentNullException(nameof(backupManager));
            _syncDetector = syncDetector ?? throw new ArgumentNullException(nameof(syncDetector));
            _syncCorrector = syncCorrector ?? throw new ArgumentNullException(nameof(syncCorrector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _minConfidenceThreshold = minConfidenceThreshold;
            _createBackups = createBackups;
        }

        /// <summary>
        /// Processes a subtitle file for synchronization issues.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <param name="mediaDuration">The duration of the media file.</param>
        /// <param name="format">The subtitle format.</param>
        /// <returns>A SyncAnalysisResult with processing information.</returns>
        public async Task<SyncAnalysisResult> ProcessSubtitleAsync(
            string subtitlePath,
            TimeSpan mediaDuration,
            SubtitleFormat format)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                throw new ArgumentException("Subtitle path cannot be null or empty.", nameof(subtitlePath));

            if (mediaDuration <= TimeSpan.Zero)
                throw new ArgumentException("Media duration must be positive.", nameof(mediaDuration));

            try
            {
                // Get the appropriate parser
                var parser = SubtitleParserFactory.GetParser(format);

                // Create detection with parser
                var detector = new SyncDetector(parser, _logger, _minConfidenceThreshold);

                // Read the subtitle file
                using var fileStream = new FileStream(subtitlePath, FileMode.Open, FileAccess.Read);

                // Detect sync issues
                var result = await detector.DetectAsync(fileStream, mediaDuration);

                if (result.IsOutOfSync && result.Confidence >= _minConfidenceThreshold)
                {
                    // Create backup if enabled
                    if (_createBackups)
                    {
                        await _backupManager.CreateBackupAsync(subtitlePath);
                    }

                    // Apply correction
                    var corrector = new SyncCorrector(parser, _logger);

                    using var correctedStream = await corrector.CorrectAsync(
                        new FileStream(subtitlePath, FileMode.Open, FileAccess.Read),
                        result.Offset);

                    // Write corrected file
                    using var outputStream = new FileStream(subtitlePath, FileMode.Create, FileAccess.Write);
                    correctedStream.Position = 0;
                    await correctedStream.CopyToAsync(outputStream);

                    result.CorrectedEntries = corrector.Correct(result.OriginalEntries, result.Offset);

                    _logger.Info("Corrected subtitles: {SubtitlePath}, offset: {Offset}, confidence: {Confidence:P0}",
                        subtitlePath, result.Offset, result.Confidence);
                }
                else
                {
                    _logger.Debug("No sync correction needed: {SubtitlePath}", subtitlePath);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing subtitles: {SubtitlePath}", subtitlePath);
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Confidence = 0,
                    DetectionMethod = SyncDetectionMethod.FirstSubtitle,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Processes a subtitle file with automatic format detection.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <param name="mediaDuration">The duration of the media file.</param>
        /// <returns>A SyncAnalysisResult with processing information.</returns>
        public async Task<SyncAnalysisResult> ProcessSubtitleAsync(
            string subtitlePath,
            TimeSpan mediaDuration)
        {
            if (string.IsNullOrWhiteSpace(subtitlePath))
                throw new ArgumentException("Subtitle path cannot be null or empty.", nameof(subtitlePath));

            // Detect format from file extension
            var extension = Path.GetExtension(subtitlePath);
            var parser = SubtitleParserFactory.GetParserByExtension(extension);

            if (parser == null)
            {
                _logger.Warn("Unsupported subtitle format: {Extension}", extension);
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Confidence = 0,
                    DetectionMethod = SyncDetectionMethod.FirstSubtitle,
                    ErrorMessage = "Unsupported subtitle format"
                };
            }

            return await ProcessSubtitleAsync(subtitlePath, mediaDuration, parser.Format);
        }

        /// <summary>
        /// Restores a subtitle file from its backup.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if restore was successful, false otherwise.</returns>
        public async Task<bool> RestoreBackupAsync(string subtitlePath)
        {
            return await _backupManager.RestoreBackupAsync(subtitlePath);
        }

        /// <summary>
        /// Deletes the backup for a subtitle file.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteBackupAsync(string subtitlePath)
        {
            return await _backupManager.DeleteBackupAsync(subtitlePath);
        }
    }
}
