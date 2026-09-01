using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Detects synchronization issues in subtitle files.
    /// </summary>
    public class SyncDetector
    {
        private readonly ISubtitleParser _parser;
        private readonly ILogger _logger;
        private readonly double _minConfidenceThreshold;
        private readonly TimeSpan _maxAllowedOffset;

        /// <summary>
        /// Initializes a new instance of the SyncDetector class.
        /// </summary>
        /// <param name="parserFactory">Factory for creating subtitle parsers.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="minConfidenceThreshold">Minimum confidence threshold for detection (0.0-1.0).</param>
        /// <param name="maxAllowedOffset">Maximum allowed offset before correction is needed.</param>
        public SyncDetector(
            ISubtitleParser parser,
            ILogger logger,
            double minConfidenceThreshold = 0.9,
            TimeSpan maxAllowedOffset = default)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _minConfidenceThreshold = minConfidenceThreshold;
            _maxAllowedOffset = maxAllowedOffset == default ? TimeSpan.FromMilliseconds(50) : maxAllowedOffset;
        }

        /// <summary>
        /// Detects synchronization issues in a subtitle file.
        /// </summary>
        /// <param name="subtitleStream">The stream containing the subtitle file.</param>
        /// <param name="mediaDuration">The duration of the media file.</param>
        /// <param name="method">The detection method to use.</param>
        /// <returns>A SyncAnalysisResult with detection information.</returns>
        public async Task<SyncAnalysisResult> DetectAsync(
            System.IO.Stream subtitleStream,
            TimeSpan mediaDuration,
            SyncDetectionMethod method = SyncDetectionMethod.FirstSubtitle)
        {
            if (subtitleStream == null)
                throw new ArgumentNullException(nameof(subtitleStream));

            if (!subtitleStream.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(subtitleStream));

            try
            {
                var entries = await _parser.ParseAsync(subtitleStream);
                var entryList = entries.ToList();

                if (!entryList.Any())
                {
                    return new SyncAnalysisResult
                    {
                        IsOutOfSync = false,
                        Confidence = 0,
                        DetectionMethod = method,
                        ErrorMessage = "No subtitle entries found."
                    };
                }

                return method switch
                {
                    SyncDetectionMethod.FirstSubtitle => await DetectByFirstSubtitleAsync(entryList, mediaDuration),
                    SyncDetectionMethod.ContentMatching => await DetectByContentMatchingAsync(entryList, mediaDuration),
                    SyncDetectionMethod.SceneDetection => await DetectBySceneDetectionAsync(entryList, mediaDuration),
                    _ => await DetectByFirstSubtitleAsync(entryList, mediaDuration)
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error detecting sync issues");
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Confidence = 0,
                    DetectionMethod = method,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Detects sync issues by analyzing the first subtitle entry.
        /// Assumes the first subtitle should appear near the beginning of the media.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectByFirstSubtitleAsync(
            List<SubtitleEntry> entries,
            TimeSpan mediaDuration)
        {
            if (!entries.Any())
                return new SyncAnalysisResult { IsOutOfSync = false, Confidence = 0, DetectionMethod = SyncDetectionMethod.FirstSubtitle };

            var firstEntry = entries.First();
            var expectedStart = TimeSpan.FromSeconds(1); // Expect first subtitle within first second
            var actualStart = firstEntry.StartTime;

            // If first subtitle starts significantly after expected
            var offset = actualStart - expectedStart;

            if (Math.Abs(offset.TotalMilliseconds) > _maxAllowedOffset.TotalMilliseconds)
            {
                // Confidence based on how far off it is
                var maxDeviation = mediaDuration.TotalMilliseconds * 0.1; // 10% of media duration
                var deviation = Math.Abs(offset.TotalMilliseconds);
                var confidence = Math.Max(0, 1 - (deviation / maxDeviation));

                return new SyncAnalysisResult
                {
                    IsOutOfSync = true,
                    Offset = offset,
                    Confidence = confidence,
                    DetectionMethod = SyncDetectionMethod.FirstSubtitle,
                    OriginalEntries = entries,
                    CorrectedEntries = entries
                };
            }

            return new SyncAnalysisResult
            {
                IsOutOfSync = false,
                Offset = TimeSpan.Zero,
                Confidence = 1.0,
                DetectionMethod = SyncDetectionMethod.FirstSubtitle,
                OriginalEntries = entries
            };
        }

        /// <summary>
        /// Detects sync issues by matching content patterns.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectByContentMatchingAsync(
            List<SubtitleEntry> entries,
            TimeSpan mediaDuration)
        {
            // This is a simplified implementation
            // In a real implementation, you would match subtitle content against
            // known patterns or audio fingerprint data

            // For now, use first subtitle method as fallback
            return await DetectByFirstSubtitleAsync(entries, mediaDuration);
        }

        /// <summary>
        /// Detects sync issues by analyzing scene changes.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectBySceneDetectionAsync(
            List<SubtitleEntry> entries,
            TimeSpan mediaDuration)
        {
            // This is a simplified implementation
            // In a real implementation, you would analyze scene change data
            // and match it with subtitle timing

            // For now, use first subtitle method as fallback
            return await DetectByFirstSubtitleAsync(entries, mediaDuration);
        }

        /// <summary>
        /// Checks if the detected offset exceeds the allowed threshold.
        /// </summary>
        public bool NeedsCorrection(TimeSpan offset)
        {
            return Math.Abs(offset.TotalMilliseconds) > _maxAllowedOffset.TotalMilliseconds;
        }
    }
}
