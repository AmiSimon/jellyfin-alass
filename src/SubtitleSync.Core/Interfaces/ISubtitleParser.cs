using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.Interfaces
{
    /// <summary>
    /// Interface for parsing and writing subtitle files.
    /// </summary>
    public interface ISubtitleParser
    {
        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        SubtitleFormat Format { get; }

        /// <summary>
        /// Parses a subtitle file from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the subtitle file.</param>
        /// <returns>A list of subtitle entries.</returns>
        Task<IEnumerable<SubtitleEntry>> ParseAsync(Stream stream);

        /// <summary>
        /// Writes subtitle entries to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="entries">The subtitle entries to write.</param>
        Task WriteAsync(Stream stream, IEnumerable<SubtitleEntry> entries);

        /// <summary>
        /// Parses a subtitle file from a string.
        /// </summary>
        /// <param name="content">The subtitle file content.</param>
        /// <returns>A list of subtitle entries.</returns>
        Task<IEnumerable<SubtitleEntry>> ParseAsync(string content);

        /// <summary>
        /// Writes subtitle entries to a string.
        /// </summary>
        /// <param name="entries">The subtitle entries to write.</param>
        /// <returns>The subtitle file content as a string.</returns>
        Task<string> WriteAsync(IEnumerable<SubtitleEntry> entries);
    }

    /// <summary>
    /// Factory for creating subtitle parsers based on format.
    /// </summary>
    public static class SubtitleParserFactory
    {
        private static readonly Dictionary<SubtitleFormat, ISubtitleParser> _parsers = new();

        /// <summary>
        /// Gets a parser for the specified format.
        /// </summary>
        /// <param name="format">The subtitle format.</param>
        /// <returns>An ISubtitleParser instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the format is not supported.</exception>
        public static ISubtitleParser GetParser(SubtitleFormat format)
        {
            if (_parsers.TryGetValue(format, out var parser))
                return parser;

            parser = format switch
            {
                SubtitleFormat.SRT => new SrtParser(),
                SubtitleFormat.ASS => new AssParser(),
                SubtitleFormat.SSA => new AssParser(),
                SubtitleFormat.WEBVTT or SubtitleFormat.VTT => new WebVttParser(),
                _ => throw new ArgumentException($"Unsupported subtitle format: {format}")
            };

            _parsers[format] = parser;
            return parser;
        }

        /// <summary>
        /// Gets all supported subtitle formats.
        /// </summary>
        public static IEnumerable<SubtitleFormat> SupportedFormats => new[]
        {
            SubtitleFormat.SRT,
            SubtitleFormat.ASS,
            SubtitleFormat.SSA,
            SubtitleFormat.WEBVTT,
            SubtitleFormat.VTT
        };

        /// <summary>
        /// Gets a parser for the specified file extension.
        /// </summary>
        /// <param name="extension">The file extension (e.g., ".srt", ".ass").</param>
        /// <returns>An ISubtitleParser instance, or null if not supported.</returns>
        public static ISubtitleParser? GetParserByExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            var ext = extension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "srt" => new SrtParser(),
                "ass" => new AssParser(),
                "ssa" => new AssParser(),
                "vtt" => new WebVttParser(),
                _ => null
            };
        }
    }
}
