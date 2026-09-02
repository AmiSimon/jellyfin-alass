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
    /// Parser for Advanced SubStation Alpha (ASS) and SubStation Alpha (SSA) subtitle formats.
    /// </summary>
    public class AssParser : SubtitleParserBase
    {
        private static readonly Regex DialogueRegex = new Regex(
            @"Dialogue:\s*(?:Marked=\d+,\s*)?(\d+:\d{2}:\d{2}.\d{2}),(\d+:\d{2}:\d{2}.\d{2}),Default,(?:[^,]+,){5}(.+)",
            RegexOptions.Compiled);

        private static readonly Regex CommentRegex = new Regex(
            @"Comment:\s*(?:Marked=\d+,\s*)?(\d+:\d{2}:\d{2}.\d{2}),(\d+:\d{2}:\d{2}.\d{2}),Default,(?:[^,]+,){5}(.+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        public override SubtitleFormat Format => SubtitleFormat.ASS;

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
            int sequence = 1;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith("//"))
                    continue;

                var match = DialogueRegex.Match(trimmed);
                if (!match.Success)
                {
                    match = CommentRegex.Match(trimmed);
                    if (!match.Success)
                        continue;
                }

                if (!TimeSpan.TryParseExact(
                    match.Groups[1].Value.Replace(".", ":"),
                    "hh:mm:ss:ff",
                    CultureInfo.InvariantCulture,
                    out TimeSpan startTime))
                {
                    continue;
                }

                if (!TimeSpan.TryParseExact(
                    match.Groups[2].Value.Replace(".", ":"),
                    "hh:mm:ss:ff",
                    CultureInfo.InvariantCulture,
                    out TimeSpan endTime))
                {
                    continue;
                }

                entries.Add(new SubtitleEntry
                {
                    SequenceNumber = sequence++,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = match.Groups[3].Value.Trim(),
                    Style = "Default"
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

            // Write ASS header
            builder.AppendLine("[V4+ Styles]");
            builder.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding");
            builder.AppendLine("Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,0,1");
            builder.AppendLine();
            builder.AppendLine("[Events]");
            builder.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

            foreach (var entry in entries)
            {
                var startStr = $"{(int)entry.StartTime.TotalHours:D2}:{entry.StartTime:mm:ss.ff}";
                var endStr = $"{(int)entry.EndTime.TotalHours:D2}:{entry.EndTime:mm:ss.ff}";

                builder.AppendLine($"Dialogue: 0,{startStr},{endStr},Default,,0,0,0,,{entry.Text}");
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Parser for SSA format (alias for ASS parser).
    /// </summary>
    public class SsaParser : AssParser
    {
        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        public override SubtitleFormat Format => SubtitleFormat.SSA;
    }
}
