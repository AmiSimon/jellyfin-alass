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
    /// Parser for WebVTT subtitle format.
    /// </summary>
    public class WebVttParser : SubtitleParserBase
    {
        private static readonly Regex TimingRegex = new Regex(
            @"(\d{2}:\d{2}:\d{2}\.\d{3})\s+-->\s+(\d{2}:\d{2}:\d{2}\.\d{3})",
            RegexOptions.Compiled);

        private static readonly Regex CueSettingsRegex = new Regex(
            @"<\d+\.\d+%>\s*(align:\S+)?\s*(position:\S+)?\s*(size:\S+)?\s*(line:\S+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override SubtitleFormat Format => SubtitleFormat.WEBVTT;

        public override async Task<IEnumerable<SubtitleEntry>> ParseAsync(string content)
        {
            var entries = new List<SubtitleEntry>();
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            bool inCue = false;
            int index = 0;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            var textBuilder = new StringBuilder();
            var styleBuilder = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Skip WEBVTT header
                if (line.Equals("WEBVTT", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("NOTE", StringComparison.OrdinalIgnoreCase))
                {
                    // If we're in a cue, save it
                    if (inCue && textBuilder.Length > 0)
                    {
                        entries.Add(new SubtitleEntry
                        {
                            Index = index++,
                            StartTime = startTime,
                            EndTime = endTime,
                            Text = textBuilder.ToString().Trim(),
                            Style = styleBuilder.Length > 0 ? styleBuilder.ToString() : null
                        });
                        inCue = false;
                        textBuilder.Clear();
                        styleBuilder.Clear();
                    }
                    continue;
                }

                // Parse timing line
                var timingMatch = TimingRegex.Match(line);
                if (timingMatch.Success)
                {
                    // Save previous cue if exists
                    if (inCue && textBuilder.Length > 0)
                    {
                        entries.Add(new SubtitleEntry
                        {
                            Index = index++,
                            StartTime = startTime,
                            EndTime = endTime,
                            Text = textBuilder.ToString().Trim(),
                            Style = styleBuilder.Length > 0 ? styleBuilder.ToString() : null
                        });
                    }

                    startTime = ParseTimeSpan(timingMatch.Groups[1].Value);
                    endTime = ParseTimeSpan(timingMatch.Groups[2].Value);
                    inCue = true;
                    textBuilder.Clear();
                    styleBuilder.Clear();
                    continue;
                }

                // Parse cue settings (optional line after timing)
                var settingsMatch = CueSettingsRegex.Match(line);
                if (inCue && !inCue && settingsMatch.Success)
                {
                    // This is a cue identifier with settings
                    styleBuilder.Append(line);
                    continue;
                }

                // If we're in a cue, this is text content
                if (inCue)
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();
                    textBuilder.Append(line);
                }
            }

            // Don't forget the last cue
            if (inCue && textBuilder.Length > 0)
            {
                entries.Add(new SubtitleEntry
                {
                    Index = index,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = textBuilder.ToString().Trim(),
                    Style = styleBuilder.Length > 0 ? styleBuilder.ToString() : null
                });
            }

            return entries;
        }

        public override async Task<string> WriteAsync(IEnumerable<SubtitleEntry> entries)
        {
            var builder = new StringBuilder();

            // Write WEBVTT header
            builder.AppendLine("WEBVTT");
            builder.AppendLine();

            foreach (var entry in entries)
            {
                // Write timing
                builder.AppendLine($"{FormatTimeSpan(entry.StartTime)} --> {FormatTimeSpan(entry.EndTime)}");

                // Write style if present
                if (!string.IsNullOrEmpty(entry.Style))
                {
                    builder.AppendLine(entry.Style);
                }

                // Write text
                var textLines = entry.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in textLines)
                {
                    builder.AppendLine(line);
                }

                // Add blank line between cues
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private TimeSpan ParseTimeSpan(string timeString)
        {
            // Format: hh:mm:ss.fff
            var parts = timeString.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4) return TimeSpan.Zero;

            var hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
            var minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
            var seconds = int.Parse(parts[2], CultureInfo.InvariantCulture);
            var milliseconds = int.Parse(parts[3], CultureInfo.InvariantCulture);

            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            // Ensure non-negative time
            if (timeSpan < TimeSpan.Zero)
                timeSpan = TimeSpan.Zero;

            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
        }
    }
}

