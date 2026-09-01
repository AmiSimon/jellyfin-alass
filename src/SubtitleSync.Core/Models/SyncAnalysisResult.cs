using System;
using System.Collections.Generic;

namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Result of analyzing subtitle synchronization.
    /// </summary>
    public class SyncAnalysisResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the subtitles are out of sync.
        /// </summary>
        public bool IsOutOfSync { get; set; }

        /// <summary>
        /// Gets or sets the detected time offset.
        /// Positive value means subtitles appear too late.
        /// Negative value means subtitles appear too early.
        /// </summary>
        public TimeSpan Offset { get; set; }

        /// <summary>
        /// Gets or sets the confidence level of the detection (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the method used for detection.
        /// </summary>
        public SyncDetectionMethod DetectionMethod { get; set; }

        /// <summary>
        /// Gets or sets the original subtitle entries (before correction).
        /// </summary>
        public IEnumerable<SubtitleEntry> OriginalEntries { get; set; } = Array.Empty<SubtitleEntry>();

        /// <summary>
        /// Gets or sets the corrected subtitle entries.
        /// </summary>
        public IEnumerable<SubtitleEntry> CorrectedEntries { get; set; } = Array.Empty<SubtitleEntry>();

        /// <summary>
        /// Gets or sets any error message from the analysis.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Methods for detecting subtitle synchronization issues.
    /// </summary>
    public enum SyncDetectionMethod
    {
        /// <summary>
        /// Detect based on the first subtitle's timing.
        /// </summary>
        FirstSubtitle,

        /// <summary>
        /// Detect based on content matching with known patterns.
        /// </summary>
        ContentMatching,

        /// <summary>
        /// Detect based on scene changes.
        /// </summary>
        SceneDetection,

        /// <summary>
        /// Manual detection.
        /// </summary>
        Manual
    }
}
