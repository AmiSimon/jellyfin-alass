using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SubtitleSync.Shared.Interfaces;
using System.Text;
using System.Threading.Tasks;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Corrects synchronization issues in subtitle files.
    /// </summary>
    public class SyncCorrector
    {
        private readonly ISubtitleParser _parser;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SyncCorrector class.
        /// </summary>
        /// <param name="parser">The subtitle parser.</param>
        /// <param name="logger">The logger.</param>
        public SyncCorrector(ISubtitleParser parser, ILogger logger)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Applies a time offset correction to a subtitle file.
        /// </summary>
        /// <param name="subtitleStream">The stream containing the subtitle file.</param>
        /// <param name="offset">The time offset to apply.</param>
        /// <returns>A stream containing the corrected subtitle file.</returns>
        public async Task<Stream> CorrectAsync(Stream subtitleStream, TimeSpan offset)
        {
            if (subtitleStream == null)
                throw new ArgumentNullException(nameof(subtitleStream));

            if (!subtitleStream.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(subtitleStream));

            if (offset == TimeSpan.Zero)
            {
                // No correction needed
                var result = new MemoryStream();
                await subtitleStream.CopyToAsync(result);
                result.Position = 0;
                return result;
            }

            try
            {
                // Parse the original subtitles
                var entries = await _parser.ParseAsync(subtitleStream);
                var entryList = entries.ToList();

                if (!entryList.Any())
                {
                    // Return empty stream if no entries
                    return new MemoryStream();
                }

                // Apply offset to all entries
                foreach (var entry in entryList)
                {
                    entry.ApplyOffset(offset);
                }

                // Write corrected subtitles
                var correctedContent = await _parser.WriteAsync(entryList);
                var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(correctedContent));
                resultStream.Position = 0;

                _logger.Info("Applied correction: {Offset} to {EntryCount} entries", offset, entryList.Count);

                return resultStream;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error correcting subtitles with offset: {Offset}", offset);
                throw;
            }
        }

        /// <summary>
        /// Applies a time offset correction to a subtitle file and writes to a specific stream.
        /// </summary>
        /// <param name="subtitleStream">The stream containing the subtitle file.</param>
        /// <param name="outputStream">The stream to write the corrected subtitles to.</param>
        /// <param name="offset">The time offset to apply.</param>
        public async Task CorrectAsync(Stream subtitleStream, Stream outputStream, TimeSpan offset)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            if (!outputStream.CanWrite)
                throw new ArgumentException("Output stream must be writable.", nameof(outputStream));

            using var correctedStream = await CorrectAsync(subtitleStream, offset);
            await correctedStream.CopyToAsync(outputStream);
            await outputStream.FlushAsync();
        }

        /// <summary>
        /// Applies a time offset correction to subtitle entries directly.
        /// </summary>
        /// <param name="entries">The subtitle entries to correct.</param>
        /// <param name="offset">The time offset to apply.</param>
        /// <returns>The corrected subtitle entries.</returns>
        public IEnumerable<SubtitleEntry> Correct(IEnumerable<SubtitleEntry> entries, TimeSpan offset)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            foreach (var entry in entries)
            {
                var corrected = entry.Clone();
                corrected.ApplyOffset(offset);
                yield return corrected;
            }
        }

        /// <summary>
        /// Validates that the correction would be valid.
        /// </summary>
        /// <param name="entries">The subtitle entries.</param>
        /// <param name="offset">The offset to apply.</param>
        /// <returns>True if the correction is valid, false otherwise.</returns>
        public bool IsValidCorrection(IEnumerable<SubtitleEntry> entries, TimeSpan offset)
        {
            if (entries == null)
                return false;

            foreach (var entry in entries)
            {
                var correctedStart = entry.StartTime.Add(offset);
                var correctedEnd = entry.EndTime.Add(offset);

                // Check if times would go negative
                if (correctedStart < TimeSpan.Zero || correctedEnd < TimeSpan.Zero)
                    return false;

                // Check if end time would be before start time
                if (correctedEnd < correctedStart)
                    return false;
            }

            return true;
        }
    }
}
