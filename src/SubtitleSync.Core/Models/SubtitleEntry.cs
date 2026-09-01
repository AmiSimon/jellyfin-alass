using System;
using System.Text;

namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Represents a single subtitle entry with timing information.
    /// </summary>
    public class SubtitleEntry
    {
        /// <summary>
        /// Gets or sets the sequence number of this entry.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the start time of this subtitle.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of this subtitle.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets or sets the text content of this subtitle.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional styling or positioning information (for ASS/SSA).
        /// </summary>
        public string Style { get; set; } = string.Empty;

        /// <summary>
        /// Returns a string representation of this subtitle entry.
        /// </summary>
        public override string ToString()
        {
            return $[{{StartTime:hh\:mm\:ss\.fff}} --> {{EndTime:hh\:mm\:ss\.fff}}] {Text}];
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
        /// Creates a deep copy of this subtitle entry.
        /// </summary>
        public SubtitleEntry Clone()
        {
            return new SubtitleEntry
            {
                SequenceNumber = SequenceNumber,
                StartTime = StartTime,
                EndTime = EndTime,
                Text = Text,
                Style = Style
            };
        }
    }
}
