using System;

namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Represents a single subtitle entry with timing information.
    /// </summary>
    public class SubtitleEntry : ICloneable
    {
        /// <summary>
        /// The sequence number of this subtitle entry.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The start time of this subtitle entry.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of this subtitle entry.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// The text content of this subtitle entry.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Additional styling or positioning information (for ASS/SSA formats).
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Creates a deep copy of this subtitle entry.
        /// </summary>
        public object Clone()
        {
            return new SubtitleEntry
            {
                Index = Index,
                StartTime = StartTime,
                EndTime = EndTime,
                Text = Text,
                Style = Style
            };
        }

        /// <summary>
        /// Returns a string representation of this subtitle entry.
        /// </summary>
        public override string ToString()
        {
            return $"[{StartTime:hh\:mm\:ss\.fff} --> {EndTime:hh\:mm\:ss\.fff}] {Text}";
        }

        /// <summary>
        /// Applies a time offset to this entry.
        /// </summary>
        /// <param name="offset">The time offset to apply.</param>
        public void ApplyOffset(TimeSpan offset)
        {
            StartTime = StartTime.Add(offset);
            EndTime = EndTime.Add(offset);

            // Ensure times don't go negative
            if (StartTime < TimeSpan.Zero)
                StartTime = TimeSpan.Zero;
            if (EndTime < StartTime)
                EndTime = StartTime;
        }

        /// <summary>
        /// Checks if this entry overlaps with another entry.
        /// </summary>
        /// <param name="other">The other entry to check against.</param>
        /// <returns>True if the entries overlap, false otherwise.</returns>
        public bool OverlapsWith(SubtitleEntry other)
        {
            return StartTime < other.EndTime && EndTime > other.StartTime;
        }

        /// <summary>
        /// Gets the duration of this subtitle entry.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
}
