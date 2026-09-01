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
    /// Parser for Advanced SubStation Alpha (ASS) subtitle format.
    /// </summary>
    public class AssParser : SubtitleParserBase
    {
        private static readonly Regex DialogueRegex = new Regex(
            @"^Dialogue:\s*\d+,\s*(?:" +
            @"(\d{1,2}:\d{2}:\d{2}\.\d{2})," +
            @"(\d{1,2}:\d{2}:\d{2}\.\d{2})," +
            @"([^,]+)," +
            @"([^,]+)," +
            @"[^,]*," +
            @"[^,]*," +
            @"[^,]*," +
            @"(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex CommentRegex = new Regex(
            @"^Comment:\s*\d+,\s*(?:" +
            @"(\d{1,2}:\d{2}:\d{2}\.\d{2})," +
            @"(\d{1,2}:\d{2}:\d{2}\.\d{2})," +
            @"([^,]+)," +
            @"([^,]+)," +
            @"[^,]*," +
            @"[^,]*," +
            @"[^,]*," +
            @"(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public override SubtitleFormat Format => SubtitleFormat.ASS;

        public override async Task<IEnumerable<SubtitleEntry>> ParseAsync(string content)
        {
            var entries = new List<SubtitleEntry>();

            // Process Dialogue lines
            var dialogueMatches = DialogueRegex.Matches(content);
            foreach (Match match in dialogueMatches)
            {
                var startTime = ParseAssTimeSpan(match.Groups[1].Value);
                var endTime = ParseAssTimeSpan(match.Groups[2].Value);
                var style = match.Groups[3].Value.Trim();
                var name = match.Groups[4].Value.Trim();
                var text = match.Groups[5].Value.Trim();

                // Clean up text by removing ASS override tags
                text = CleanAssText(text);

                entries.Add(new SubtitleEntry
                {
                    Index = entries.Count + 1,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = text,
                    Style = style
                });
            }

            // Process Comment lines (also can contain subtitles)
            var commentMatches = CommentRegex.Matches(content);
            foreach (Match match in commentMatches)
            {
                var startTime = ParseAssTimeSpan(match.Groups[1].Value);
                var endTime = ParseAssTimeSpan(match.Groups[2].Value);
                var style = match.Groups[3].Value.Trim();
                var name = match.Groups[4].Value.Trim();
                var text = match.Groups[5].Value.Trim();

                // Clean up text
                text = CleanAssText(text);

                entries.Add(new SubtitleEntry
                {
                    Index = entries.Count + 1,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = text,
                    Style = style
                });
            }

            return entries;
        }

        public override async Task<string> WriteAsync(IEnumerable<SubtitleEntry> entries)
        {
            var builder = new StringBuilder();

            // Write ASS header
            builder.AppendLine("[Script Info]");
            builder.AppendLine("Title: SubtitleSync");
            builder.AppendLine("ScriptType: v4.00+");
            builder.AppendLine();

            builder.AppendLine("[V4+ Styles]");
            builder.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
            builder.AppendLine("Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,2,0,0,0,1");
            builder.AppendLine();

            builder.AppendLine("[Events]");
            builder.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

            foreach (var entry in entries)
            {
                var style = string.IsNullOrEmpty(entry.Style) ? "Default" : entry.Style;
                var cleanedText = EscapeAssText(entry.Text);

                builder.AppendLine(
                    $"Dialogue: 0,{FormatAssTimeSpan(entry.StartTime)},{FormatAssTimeSpan(entry.EndTime)},{style},,0,0,0,,{cleanedText}");
            }

            return builder.ToString();
        }

        private TimeSpan ParseAssTimeSpan(string timeString)
        {
            // ASS format: hh:mm:ss.cc (centiseconds)
            // But can also be h:mm:ss.cc or mm:ss.cc
            var parts = timeString.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);

            int hours = 0, minutes = 0, seconds = 0, centiseconds = 0;

            switch (parts.Length)
            {
                case 2: // mm:ss.cc
                    minutes = int.Parse(parts[0], CultureInfo.InvariantCulture);
                    ParseSecondsAndCentiseconds(parts[1], out seconds, out centiseconds);
                    break;
                case 3: // h:mm:ss.cc or hh:mm:ss.cc
                    if (parts[0].Length <= 2) // h:mm:ss.cc
                    {
                        hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        ParseSecondsAndCentiseconds(parts[2], out seconds, out centiseconds);
                    }
                    else // hh:mm:ss.cc
                    {
                        hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        ParseSecondsAndCentiseconds(parts[2], out seconds, out centiseconds);
                    }
                    break;
                case 4: // hh:mm:ss.cc
                    hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
                    minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
                    ParseSecondsAndCentiseconds(parts[2] + "." + parts[3], out seconds, out centiseconds);
                    break;
            }

            return new TimeSpan(0, hours, minutes, seconds, centiseconds * 10);
        }

        private void ParseSecondsAndCentiseconds(string value, out int seconds, out int centiseconds)
        {
            var secParts = value.Split('.');
            seconds = int.Parse(secParts[0], CultureInfo.InvariantCulture);
            centiseconds = secParts.Length > 1 ? int.Parse(secParts[1], CultureInfo.InvariantCulture) : 0;
        }

        private string FormatAssTimeSpan(TimeSpan timeSpan)
        {
            // Ensure non-negative time
            if (timeSpan < TimeSpan.Zero)
                timeSpan = TimeSpan.Zero;

            // ASS uses centiseconds (1/100 of a second)
            var totalMilliseconds = (int)timeSpan.TotalMilliseconds;
            var hours = totalMilliseconds / (60 * 60 * 1000);
            var remaining = totalMilliseconds % (60 * 60 * 1000);
            var minutes = remaining / (60 * 1000);
            remaining = remaining % (60 * 1000);
            var seconds = remaining / 1000;
            var centiseconds = (remaining % 1000) / 10;

            // Format: hh:mm:ss.cc
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{centiseconds:D2}";
        }

        private string CleanAssText(string text)
        {
            // Remove ASS override tags like {\fn...}, {\fs...}, etc.
            // Keep basic formatting like {\b1}, {\i1}, {\u1}, {\s1}
            text = Regex.Replace(text, @"{\[^\}]+}", string.Empty);
            return text.Trim();
        }

        private string EscapeAssText(string text)
        {
            // Escape special characters for ASS
            text = text.Replace("{", "{{");
            text = text.Replace("}", "}}");
            text = text.Replace("\n", "\N");
            return text;
        }
    }

    /// <summary>
    /// Parser for SubStation Alpha (SSA) subtitle format.
    /// </summary>
    public class SsaParser : AssParser
    {
        public override SubtitleFormat Format => SubtitleFormat.SSA;
    }
}

