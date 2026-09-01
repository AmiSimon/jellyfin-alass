namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Enumeration of supported subtitle formats.
    /// </summary>
    public enum SubtitleFormat
    {
        /// <summary>
        /// Unknown or unsupported format.
        /// </summary>
        Unknown,

        /// <summary>
        /// SubRip Text format (.srt)
        /// </summary>
        SRT,

        /// <summary>
        /// Advanced SubStation Alpha format (.ass)
        /// </summary>
        ASS,

        /// <summary>
        /// SubStation Alpha format (.ssa)
        /// </summary>
        SSA,

        /// <summary>
        /// WebVTT format (.vtt)
        /// </summary>
        WEBVTT,

        /// <summary>
        /// WebVTT format (alternative extension)
        /// </summary>
        VTT
    }

    /// <summary>
    /// Helper class for subtitle format operations.
    /// </summary>
    public static class SubtitleFormatHelper
    {
        /// <summary>
        /// Gets the file extensions associated with a subtitle format.
        /// </summary>
        /// <param name="format">The subtitle format.</param>
        /// <returns>Array of file extensions (including the dot).</returns>
        public static string[] GetExtensions(SubtitleFormat format)
        {
            return format switch
            {
                SubtitleFormat.SRT => new[] { ".srt" },
                SubtitleFormat.ASS => new[] { ".ass" },
                SubtitleFormat.SSA => new[] { ".ssa" },
                SubtitleFormat.WEBVTT => new[] { ".vtt", ".webvtt" },
                SubtitleFormat.VTT => new[] { ".vtt" },
                _ => new string[0]
            };
        }

        /// <summary>
        /// Detects the subtitle format from a file extension.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The detected subtitle format.</returns>
        public static SubtitleFormat DetectFromExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return SubtitleFormat.Unknown;

            var extension = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();

            return extension switch
            {
                ".srt" => SubtitleFormat.SRT,
                ".ass" => SubtitleFormat.ASS,
                ".ssa" => SubtitleFormat.SSA,
                ".vtt" or ".webvtt" => SubtitleFormat.WEBVTT,
                _ => SubtitleFormat.Unknown
            };
        }

        /// <summary>
        /// Detects the subtitle format from file content.
        /// </summary>
        /// <param name="content">The file content as a string.</param>
        /// <returns>The detected subtitle format.</returns>
        public static SubtitleFormat DetectFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return SubtitleFormat.Unknown;

            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Check for WebVTT signature
            if (lines.Length > 0 && lines[0].Trim().Equals("WEBVTT", StringComparison.OrdinalIgnoreCase))
                return SubtitleFormat.WEBVTT;

            // Check for ASS/SSA signature
            if (lines.Length > 0 && lines[0].Trim().StartsWith("[Script Info]", StringComparison.OrdinalIgnoreCase))
                return SubtitleFormat.ASS;
            if (lines.Length > 0 && lines[0].Trim().StartsWith("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
                return SubtitleFormat.SSA;

            // Check for SRT pattern (number followed by timing)
            if (lines.Length >= 3)
            {
                // SRT entries typically start with a number
                if (int.TryParse(lines[0].Trim(), out _))
                {
                    // Next line should be timing: 00:00:00,000 --> 00:00:00,000
                    var timingLine = lines[1].Trim();
                    if (timingLine.Contains("-->") && timingLine.Contains(":"))
                        return SubtitleFormat.SRT;
                }
            }

            return SubtitleFormat.Unknown;
        }

        /// <summary>
        /// Gets the MIME type for a subtitle format.
        /// </summary>
        /// <param name="format">The subtitle format.</param>
        /// <returns>The MIME type string.</returns>
        public static string GetMimeType(SubtitleFormat format)
        {
            return format switch
            {
                SubtitleFormat.SRT => "application/x-subrip",
                SubtitleFormat.ASS or SubtitleFormat.SSA => "text/x-ssa",
                SubtitleFormat.WEBVTT or SubtitleFormat.VTT => "text/vtt",
                _ => "text/plain"
            };
        }
    }
}
