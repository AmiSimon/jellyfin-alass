using System;
using System.Collections.Generic;

namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Represents the result of a subtitle sync analysis.
    /// </summary>
    public class SyncAnalysisResult
    {
        /// <summary>
        /// Whether the subtitle is out of sync.
        /// </summary>
        public bool IsOutOfSync { get; set; }

        /// <summary>
        /// The detected time offset that needs to be applied.
        /// Positive value means subtitles appear too early (need to be delayed).
        /// Negative value means subtitles appear too late (need to be moved earlier).
        /// </summary>
        public TimeSpan Offset { get; set; }

        /// <summary>
        /// Confidence level of the detection (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The detection method used.
        /// </summary>
        public SyncDetectionMethod Method { get; set; } = SyncDetectionMethod.Unknown;

        /// <summary>
        /// Additional details about the analysis.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// List of individual entry analyses (for detailed reporting).
        /// </summary>
        public List<EntryAnalysis> EntryAnalyses { get; set; } = new List<EntryAnalysis>();

        /// <summary>
        /// The media duration used for analysis.
        /// </summary>
        public TimeSpan MediaDuration { get; set; }

        /// <summary>
        /// The number of subtitle entries analyzed.
        /// </summary>
        public int EntriesAnalyzed { get; set; }

        /// <summary>
        /// Returns a string representation of this analysis result.
        /// </summary>
        public override string ToString()
        {
            return $"SyncAnalysis: OutOfSync={IsOutOfSync}, Offset={Offset.TotalMilliseconds:F2}ms, " +
                   $"Confidence={Confidence:P2}, Method={Method}, Entries={EntriesAnalyzed}";
        }
    }

    /// <summary>
    /// Analysis result for a single subtitle entry.
    /// </summary>
    public class EntryAnalysis
    {
        /// <summary>
        /// The index of the entry.
        /// </summary>
        public int EntryIndex { get; set; }

        /// <summary>
        /// The expected start time.
        /// </summary>
        public TimeSpan ExpectedStartTime { get; set; }

        /// <summary>
        /// The actual start time.
        /// </summary>
        public TimeSpan ActualStartTime { get; set; }

        /// <summary>
        /// The calculated offset for this entry.
        /// </summary>
        public TimeSpan Offset { get; set; }

        /// <summary>
        /// Whether this entry is out of sync.
        /// </summary>
        public bool IsOutOfSync { get; set; }

        /// <summary>
        /// The confidence for this specific entry.
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Methods used for sync detection.
    /// </summary>
    public enum SyncDetectionMethod
    {
        Unknown,
        FirstSubtitle,
        ContentMatching,
        SceneDetection,
        ManualOffset,
        ReferenceFile
    }
}
