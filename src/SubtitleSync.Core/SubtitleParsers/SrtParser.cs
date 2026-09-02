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
    /// Parser for SubRip Text (SRT) subtitle format.
    /// </summary>
    public class SrtParser : SubtitleParserBase
    {
        private static readonly Regex TimingLineRegex = new Regex(
            @"(\d{2}:\d{2}:\d{2},\d{3})\s+-->\s+(\d{2}:\d{2}:\d{2},\d{3})",
            RegexOptions.Compiled);

        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        public override SubtitleFormat Format => SubtitleFormat.SRT;

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

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Parse sequence number
                if (!int.TryParse(line, out int sequenceNumber))
                    continue;

                // Next line should be timing
                if (i + 1 >= lines.Length)
                    break;

                var timingLine = lines[++i].Trim();
                var timingMatch = TimingLineRegex.Match(timingLine);
                if (!timingMatch.Success)
                    continue;

                // Parse start and end times
                if (!TimeSpan.TryParseExact(
                    timingMatch.Groups[1].Value.Replace(",", "."),
                    new[] { "hh:mm:ss.fff", "hh:mm:ss,fff" },
                    CultureInfo.InvariantCulture,
                    out TimeSpan startTime))
                {
                    continue;
                }

                if (!TimeSpan.TryParseExact(
                    timingMatch.Groups[2].Value.Replace(",", "."),
                    new[] { "hh:mm:ss.fff", "hh:mm:ss,fff" },
                    CultureInfo.InvariantCulture,
                    out TimeSpan endTime))
                {
                    continue;
                }

                // Collect text lines
                var textBuilder = new StringBuilder();
                bool firstTextLine = true;

                while (i + 1 < lines.Length)
                {
                    var nextLine = lines[++i].Trim();
                    if (string.IsNullOrWhiteSpace(nextLine))
                        break;

                    if (!firstTextLine)
                        textBuilder.AppendLine();

                    textBuilder.Append(nextLine);
                    firstTextLine = false;
                }

                entries.Add(new SubtitleEntry
                {
                    SequenceNumber = sequenceNumber,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = textBuilder.ToString()
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
            int index = 1;

            foreach (var entry in entries)
            {
                builder.AppendLine(index.ToString());
                builder.AppendLine($"{entry.StartTime:hh:mm:ss,fff} --> {entry.EndTime:hh:mm:ss,fff}");
                builder.AppendLine(entry.Text);
                builder.AppendLine();
                index++;
            }

            return builder.ToString();
        }
    }
}
