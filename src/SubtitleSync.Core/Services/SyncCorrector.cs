using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Corrects synchronization issues in subtitle files by applying time offsets.
    /// </summary>
    public class SyncCorrector
    {
        private readonly ISubtitleParser _parser;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SyncCorrector class.
        /// </summary>
        /// <param name="parser">The subtitle parser to use.</param>
        /// <param name="logger">The logger.</param>
        public SyncCorrector(ISubtitleParser parser, ILogger logger)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Corrects a subtitle file by applying the specified offset.
        /// </summary>
        /// <param name="subtitleStream">The subtitle file stream.</param>
        /// <param name="offset">The time offset to apply.</param>
        /// <returns>A stream containing the corrected subtitle file.</returns>
        public async Task<Stream> CorrectAsync(Stream subtitleStream, TimeSpan offset)
        {
            var entries = await _parser.ParseAsync(subtitleStream);
            var correctedEntries = ApplyOffsetToEntries(entries, offset);
            
            var resultStream = new MemoryStream();
            await _parser.WriteAsync(resultStream, correctedEntries);
            resultStream.Position = 0;
            return resultStream;
        }

        /// <summary>
        /// Applies a time offset to all subtitle entries.
        /// </summary>
        /// <param name="entries">The subtitle entries to correct.</param>
        /// <param name="offset">The time offset to apply.</param>
        /// <returns>A list of corrected subtitle entries.</returns>
        public IEnumerable<SubtitleEntry> ApplyOffsetToEntries(
            IEnumerable<SubtitleEntry> entries, TimeSpan offset)
        {
            var correctedEntries = new List<SubtitleEntry>();
            var entryList = entries.ToList();

            foreach (var entry in entryList)
            {
                var correctedEntry = (SubtitleEntry)entry.Clone();
                correctedEntry.ApplyOffset(offset);
                correctedEntries.Add(correctedEntry);
            }

            // Fix any overlapping entries caused by the offset
            correctedEntries = FixOverlappingEntries(correctedEntries);

            // Re-index entries
            for (int i = 0; i < correctedEntries.Count; i++)
            {
                correctedEntries[i].Index = i + 1;
            }

            return correctedEntries;
        }

        /// <summary>
        /// Fixes overlapping entries by adjusting their timings.
        /// </summary>
        /// <param name="entries">The list of entries to fix.</param>
        /// <returns>A list of entries with no overlaps.</returns>
        private List<SubtitleEntry> FixOverlappingEntries(List<SubtitleEntry> entries)
        {
            if (entries.Count <= 1)
                return entries;

            var fixedEntries = new List<SubtitleEntry> { entries[0] };

            for (int i = 1; i < entries.Count; i++)
            {
                var current = entries[i];
                var previous = fixedEntries[^1];

                // If current starts before previous ends, adjust it
                if (current.StartTime < previous.EndTime)
                {
                    // Move current to start at previous end time
                    var adjustment = previous.EndTime - current.StartTime;
                    current.StartTime = previous.EndTime;
                    current.EndTime = current.EndTime.Add(adjustment);

                    _logger.Warn($