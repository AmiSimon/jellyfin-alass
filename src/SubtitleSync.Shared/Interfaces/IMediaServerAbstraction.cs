using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubtitleSync.Shared.Interfaces
{
    /// <summary>
    /// Abstraction for media server operations (Jellyfin/Emby).
    /// </summary>
    public interface IMediaServerAbstraction
    {
        /// <summary>
        /// Gets the name of the media server.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Gets the version of the media server.
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Gets the plugin configuration directory path.
        /// </summary>
        string PluginConfigurationDirectory { get; }

        /// <summary>
        /// Gets the plugin data directory path.
        /// </summary>
        string PluginDataDirectory { get; }

        /// <summary>
        /// Gets all media items that have subtitle files.
        /// </summary>
        /// <returns>A collection of media items with subtitles.</returns>
        Task<IEnumerable<IMediaItem>> GetMediaItemsWithSubtitlesAsync();

        /// <summary>
        /// Gets the subtitle files for a specific media item.
        /// </summary>
        /// <param name="mediaItemId">The media item ID.</param>
        /// <returns>A collection of subtitle file paths.</returns>
        Task<IEnumerable<string>> GetSubtitleFilesAsync(string mediaItemId);

        /// <summary>
        /// Gets the duration of a media item.
        /// </summary>
        /// <param name="mediaItemId">The media item ID.</param>
        /// <returns>The media duration.</returns>
        Task<TimeSpan> GetMediaDurationAsync(string mediaItemId);

        /// <summary>
        /// Gets the file path for a subtitle.
        /// </summary>
        /// <param name="subtitleId">The subtitle ID.</param>
        /// <returns>The full file path.</returns>
        Task<string> GetSubtitleFilePathAsync(string subtitleId);

        /// <summary>
        /// Sends a notification to the user.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="message">The notification message.</param>
        Task SendNotificationAsync(string title, string message);

        /// <summary>
        /// Logs a message to the server log.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        Task LogAsync(LogLevel level, string message, params object[] args);
    }

    /// <summary>
    /// Represents a media item.
    /// </summary>
    public interface IMediaItem
    {
        /// <summary>
        /// Gets the unique identifier of the media item.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the media item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the media item (Movie, Episode, etc.).
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the path to the media file.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the duration of the media item.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the date the media was added.
        /// </summary>
        DateTime DateAdded { get; }
    }

    /// <summary>
    /// Log levels for media server logging.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}
