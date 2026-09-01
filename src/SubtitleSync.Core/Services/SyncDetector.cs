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

        /// <summary>
        /// Initializes a new instance of the SyncDetector class.
        /// </summary>
        /// <param name="parser">The subtitle parser to use.</param>
        /// <param name="logger">The logger.</param>
        public SyncDetector(ISubtitleParser parser, ILogger logger)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Detects sync issues in a subtitle file.
        /// </summary>
        /// <param name="subtitleStream">The subtitle file stream.</param>
        /// <param name="mediaDuration">The duration of the media file.</param>
        /// <param name="detectionMethod">The detection method to use.</param>
        /// <returns>A SyncAnalysisResult containing the detection results.</returns>
        public async Task<SyncAnalysisResult> DetectAsync(
            System.IO.Stream subtitleStream,
            TimeSpan mediaDuration,
            SyncDetectionMethod detectionMethod = SyncDetectionMethod.FirstSubtitle)
        {
            var entries = await _parser.ParseAsync(subtitleStream);
            var entryList = entries.ToList();

            if (entryList.Count == 0)
            {
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Offset = TimeSpan.Zero,
                    Confidence = 1.0,
                    Method = detectionMethod,
                    MediaDuration = mediaDuration,
                    EntriesAnalyzed = 0,
                    Details = "No subtitle entries found"
                };
            }

            SyncAnalysisResult result = detectionMethod switch
            {
                SyncDetectionMethod.FirstSubtitle => await DetectByFirstSubtitle(entryList, mediaDuration),
                SyncDetectionMethod.ContentMatching => await DetectByContentMatching(entryList, mediaDuration),
                SyncDetectionMethod.SceneDetection => await DetectBySceneDetection(entryList, mediaDuration),
                _ => await DetectByFirstSubtitle(entryList, mediaDuration)
            };

            result.MediaDuration = mediaDuration;
            result.EntriesAnalyzed = entryList.Count;
            result.Method = detectionMethod;

            return result;
        }

        /// <summary>
        /// Detects sync issues by analyzing the first subtitle entry.
        /// Assumes that the first subtitle should appear at or near the start of the media.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectByFirstSubtitle(List<SubtitleEntry> entries, TimeSpan mediaDuration)
        {
            if (entries.Count == 0)
                return new SyncAnalysisResult { IsOutOfSync = false, Offset = TimeSpan.Zero, Confidence = 1.0 };

            var firstEntry = entries[0];

            // If the first subtitle starts significantly after 0, it might be out of sync
            // This is a simple heuristic - in reality, the first subtitle might legitimately start later
            // We use a conservative threshold of 10 seconds
            var threshold = TimeSpan.FromSeconds(10);

            if (firstEntry.StartTime > threshold)
            {
                // The subtitle starts too late - it might need to be moved earlier
                // But this could also be legitimate (e.g., movie with no dialogue at the start)
                // So we return low confidence
                return new SyncAnalysisResult
                {
                    IsOutOfSync = true,
                    Offset = -firstEntry.StartTime, // Negative offset means move earlier
                    Confidence = 0.3, // Low confidence - this might be wrong
                    Details = $"First subtitle starts at {firstEntry.StartTime.TotalSeconds:F1}s (threshold: {threshold.TotalSeconds}s)"
                };
            }

            // If the first subtitle starts before 0, it's definitely out of sync
            if (firstEntry.StartTime < TimeSpan.Zero)
            {
                return new SyncAnalysisResult
                {
                    IsOutOfSync = true,
                    Offset = -firstEntry.StartTime, // Positive offset means delay
                    Confidence = 1.0,
                    Details = $"First subtitle has negative start time: {firstEntry.StartTime.TotalMilliseconds}ms"
                };
            }

            // Check if subtitles end after media duration
            var lastEntry = entries[^1];
            if (lastEntry.EndTime > mediaDuration)
            {
                var excess = lastEntry.EndTime - mediaDuration;
                return new SyncAnalysisResult
                {
                    IsOutOfSync = true,
                    Offset = -excess, // Move subtitles earlier
                    Confidence = 0.8,
                    Details = $"Last subtitle extends beyond media duration by {excess.TotalSeconds:F1}s"
                };
            }

            // No obvious sync issues detected
            return new SyncAnalysisResult
            {
                IsOutOfSync = false,
                Offset = TimeSpan.Zero,
                Confidence = 1.0,
                Details = "No sync issues detected"
            };
        }

        /// <summary>
        /// Detects sync issues by comparing subtitle content with expected audio cues.
        /// This is a placeholder - in a real implementation, this would use audio analysis.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectByContentMatching(List<SubtitleEntry> entries, TimeSpan mediaDuration)
        {
            // This is a simplified implementation
            // A real implementation would:
            // 1. Extract audio fingerprints or transcripts
            // 2. Match subtitle text with audio content
            // 3. Calculate timing offsets based on matches

            // For now, we'll use a heuristic based on subtitle distribution
            return await DetectByDistributionAnalysis(entries, mediaDuration);
        }

        /// <summary>
        /// Detects sync issues by analyzing the distribution of subtitles.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectByDistributionAnalysis(List<SubtitleEntry> entries, TimeSpan mediaDuration)
        {
            if (entries.Count < 10) // Not enough data for meaningful analysis
                return await DetectByFirstSubtitle(entries, mediaDuration);

            // Calculate expected distribution
            var totalSubtitleTime = TimeSpan.Zero;
            foreach (var entry in entries)
            {
                totalSubtitleTime += entry.Duration;
            }

            // Expected: subtitles should cover a reasonable portion of the media
            // Typical movies: 30-60% of runtime has subtitles
            var coverageRatio = totalSubtitleTime.TotalSeconds / mediaDuration.TotalSeconds;

            if (coverageRatio > 0.8)
            {
                // Too many subtitles - might be out of sync (overlapping or extended)
                return new SyncAnalysisResult
                {
                    IsOutOfSync = true,
                    Offset = TimeSpan.Zero, // Can't determine offset from this alone
                    Confidence = 0.5,
                    Details = $"Unusually high subtitle coverage: {coverageRatio:P0}"
                };
            }

            if (coverageRatio < 0.1)
            {
                // Too few subtitles
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false, // This might be legitimate (e.g., silent movie)
                    Offset = TimeSpan.Zero,
                    Confidence = 0.8,
                    Details = $"Low subtitle coverage: {coverageRatio:P0}"
                };
            }

            // Check for consistent gaps between subtitles
            var gaps = new List<TimeSpan>();
            for (int i = 1; i < entries.Count; i++)
            {
                var gap = entries[i].StartTime - entries[i - 1].EndTime;
                if (gap > TimeSpan.Zero)
                    gaps.Add(gap);
            }

            if (gaps.Count > 0)
            {
                var avgGap = gaps.Average(g => g.TotalSeconds);
                var maxGap = gaps.Max(g => g.TotalSeconds);

                // If there are very large gaps, subtitles might be misaligned
                if (maxGap > 60 && avgGap < 5)
                {
                    return new SyncAnalysisResult
                    {
                        IsOutOfSync = true,
                        Offset = TimeSpan.Zero,
                        Confidence = 0.6,
                        Details = $"Inconsistent gaps detected (max: {maxGap:F1}s, avg: {avgGap:F1}s)"
                    };
                }
            }

            return new SyncAnalysisResult
            {
                IsOutOfSync = false,
                Offset = TimeSpan.Zero,
                Confidence = 0.9,
                Details = "Distribution analysis passed"
            };
        }

        /// <summary>
        /// Detects sync issues by analyzing scene changes.
        /// This is a placeholder - in a real implementation, this would use video analysis.
        /// </summary>
        private async Task<SyncAnalysisResult> DetectBySceneDetection(List<SubtitleEntry> entries, TimeSpan mediaDuration)
        {
            // This would require video analysis to detect scene changes
            // and correlate them with subtitle timing
            // For now, fall back to first subtitle detection
            return await DetectByFirstSubtitle(entries, mediaDuration);
        }

        /// <summary>
        /// Detects sync issues using a reference subtitle file.
        /// </summary>
        public async Task<SyncAnalysisResult> DetectWithReferenceAsync(
            System.IO.Stream subtitleStream,
            System.IO.Stream referenceStream,
            SubtitleFormat format)
        {
            var subtitleEntries = (await _parser.ParseAsync(subtitleStream)).ToList();
            var referenceParser = SubtitleParserFactory.GetParser(format);
            var referenceEntries = (await referenceParser.ParseAsync(referenceStream)).ToList();

            if (subtitleEntries.Count == 0 || referenceEntries.Count == 0)
            {
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Offset = TimeSpan.Zero,
                    Confidence = 1.0,
                    Method = SyncDetectionMethod.ReferenceFile,
                    Details = "No entries in one or both files"
                };
            }

            // Find matching entries based on text content
            var matches = new List<(SubtitleEntry Sub, SubtitleEntry Ref, TimeSpan Offset)>();

            foreach (var subEntry in subtitleEntries)
            {
                var refEntry = referenceEntries.FirstOrDefault(r =>
                    string.Equals(r.Text, subEntry.Text, StringComparison.OrdinalIgnoreCase));

                if (refEntry != null)
                {
                    var offset = subEntry.StartTime - refEntry.StartTime;
                    matches.Add((subEntry, refEntry, offset));
                }
            }

            if (matches.Count < 3) // Need at least 3 matches for reliable detection
            {
                return new SyncAnalysisResult
                {
                    IsOutOfSync = false,
                    Offset = TimeSpan.Zero,
                    Confidence = 0.5,
                    Method = SyncDetectionMethod.ReferenceFile,
                    Details = $"Only {matches.Count} matching entries found"
                };
            }

            // Calculate average offset
            var avgOffset = TimeSpan.FromTicks((long)matches.Average(m => m.Offset.Ticks));
            var confidence = Math.Min(1.0, matches.Count / 10.0); // More matches = higher confidence

            return new SyncAnalysisResult
            {
                IsOutOfSync = Math.Abs(avgOffset.TotalMilliseconds) > 10, // More than 10ms difference
                Offset = avgOffset,
                Confidence = confidence,
                Method = SyncDetectionMethod.ReferenceFile,
                Details = $"Reference comparison: {matches.Count} matches, avg offset: {avgOffset.TotalMilliseconds:F2}ms"
            };
        }
    }
}
