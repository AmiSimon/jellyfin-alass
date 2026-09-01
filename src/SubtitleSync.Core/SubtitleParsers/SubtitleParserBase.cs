using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.SubtitleParsers
{
    /// <summary>
    /// Base class for subtitle parsers providing common functionality.
    /// </summary>
    public abstract class SubtitleParserBase : ISubtitleParser
    {
        /// <summary>
        /// Gets the subtitle format this parser handles.
        /// </summary>
        public abstract SubtitleFormat Format { get; }

        /// <summary>
        /// Parses a subtitle file from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the subtitle file.</param>
        /// <returns>A list of subtitle entries.</returns>
        public async Task<IEnumerable<SubtitleEntry>> ParseAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(stream));

            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            var content = await reader.ReadToEndAsync();
            return await ParseAsync(content);
        }

        /// <summary>
        /// Writes subtitle entries to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="entries">The subtitle entries to write.</param>
        public async Task WriteAsync(Stream stream, IEnumerable<SubtitleEntry> entries)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writable.", nameof(stream));

            var content = await WriteAsync(entries);
            var bytes = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        /// <summary>
        /// Parses a subtitle file from a string.
        /// </summary>
        /// <param name="content">The subtitle file content.</param>
        /// <returns>A list of subtitle entries.</returns>
        public abstract Task<IEnumerable<SubtitleEntry>> ParseAsync(string content);

        /// <summary>
        /// Writes subtitle entries to a string.
        /// </summary>
        /// <param name="entries">The subtitle entries to write.</param>
        /// <returns>The subtitle file content as a string.</returns>
        public abstract Task<string> WriteAsync(IEnumerable<SubtitleEntry> entries);
    }
}
