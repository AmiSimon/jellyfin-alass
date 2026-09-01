using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.SubtitleParsers
{
    /// <summary>
    /// Parser for Web Video Text Tracks (WebVTT) subtitle format.
    /// </summary>
    public class WebVttParser : SubtitleParserBase
    {
        private static readonly Regex TimingRegex = new Regex(
            @"(\d{2}:\d{2}:\d{2}\.\d{3})\s+--\>\s+(\d{2}:\d{2}:\d{2}\.\d{3})",
            RegexOptions.Compiled);

        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        public override SubtitleFormat Format => SubtitleFormat.WEBVTT;

        /// <summary>
        /// Parses a subtitle file from a string.
        /// </summary>
        /// <param name="content">The subtitle file content.</param>
        /// <returns>A list of subtitle entries.</returns>
        public override async Task<IEnumerable<SubtitleEntry>> ParseAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return Array.Empty<SubtitleEntry>();

            var entries = new List<SubtitleEntry>();
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            bool inCue = false;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            var textBuilder = new StringBuilder();
            int sequence = 1;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip WebVTT header
                if (trimmed.Equals("WEBVTT", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("WEBVTT ", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip comments
                if (trimmed.StartsWith("NOTE") || trimmed.StartsWith("-->"))
                    continue;

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    if (inCue && textBuilder.Length > 0)
                    {
                        entries.Add(new SubtitleEntry
                        {
                            SequenceNumber = sequence++,
                            StartTime = startTime,
                            EndTime = endTime,
                            Text = textBuilder.ToString().Trim()
                        });

                        inCue = false;
                        textBuilder.Clear();
                    }
                    continue;
                }

                // Parse timing line
                var timingMatch = TimingRegex.Match(trimmed);
                if (timingMatch.Success)
                {
                    if (!TimeSpan.TryParseExact(
                        timingMatch.Groups[1].Value,
                        "hh\:mm\:ss\.fff",
                        CultureInfo.InvariantCulture,
                        out startTime))
                    {
                        continue;
                    }

                    if (!TimeSpan.TryParseExact(
                        timingMatch.Groups[2].Value,
                        "hh\:mm\:ss\.fff",
                        CultureInfo.InvariantCulture,
                        out endTime))
                    {
                        continue;
                    }

                    inCue = true;
                    textBuilder.Clear();
                    continue;
                }

                // Text line
                if (inCue)
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();
                    textBuilder.Append(trimmed);
                }
            }

            // Add last entry if exists
            if (inCue && textBuilder.Length > 0)
            {
                entries.Add(new SubtitleEntry
                {
                    SequenceNumber = sequence,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = textBuilder.ToString().Trim()
                });
            }

            return entries;
        }

        /// <summary>
        /// Writes subtitle entries to a string.
        /// </summary>
        /// <param name="entries">The subtitle entries to write.</param>
        /// <returns>The subtitle file content as a string.</returns>
        public override async Task<string> WriteAsync(IEnumerable<SubtitleEntry> entries)
        {
            var builder = new StringBuilder();
            builder.AppendLine("WEBVTT");
            builder.AppendLine();

            foreach (var entry in entries)
            {
                builder.AppendLine($"{entry.StartTime:hh\:mm\:ss\.fff} --> {entry.EndTime:hh\:mm\:ss\.fff}");
                builder.AppendLine(entry.Text);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
