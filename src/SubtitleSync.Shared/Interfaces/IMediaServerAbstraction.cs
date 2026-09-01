using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SubtitleSync.Shared.Interfaces
{
    /// <summary>
    /// Unified interface for accessing media server functionality.
    /// Implemented by both Jellyfin and Emby adapters.
    /// </summary>
    public interface IMediaServerAbstraction
    {
        /// <summary>
        /// Gets all media items from the library
        /// </summary>
        Task<IEnumerable<MediaItem>> GetMediaItemsAsync();

        /// <summary>
        /// Gets a specific media item by ID
        /// </summary>
        Task<MediaItem> GetMediaItemAsync(string itemId);

        /// <summary>
        /// Gets the media stream for a file
        /// </summary>
        Task<Stream> GetMediaStreamAsync(string path);

        /// <summary>
        /// Gets the subtitle stream for a file
        /// </summary>
        Task<Stream> GetSubtitleStreamAsync(string path);

        /// <summary>
        /// Saves a subtitle file
        /// </summary>
        Task SaveSubtitleAsync(string path, Stream content);

        /// <summary>
        /// Gets all subtitle files for a media item
        /// </summary>
        Task<IEnumerable<SubtitleFile>> GetSubtitleFilesAsync(string itemId);

        /// <summary>
        /// Gets plugin configuration
        /// </summary>
        Task<PluginConfiguration> GetConfigurationAsync();

        /// <summary>
        /// Saves plugin configuration
        /// </summary>
        Task SaveConfigurationAsync(PluginConfiguration config);

        /// <summary>
        /// Gets the server's logger
        /// </summary>
        ILogger GetLogger();

        /// <summary>
        /// Gets the server version
        /// </summary>
        string GetServerVersion();

        /// <summary>
        /// Gets a service of the specified type
        /// </summary>
        T GetService<T>() where T : class;
    }

    /// <summary>
    /// Represents a media item in the library
    /// </summary>
    public class MediaItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public MediaType Type { get; set; }
    }

    /// <summary>
    /// Represents a subtitle file
    /// </summary>
    public class SubtitleFile
    {
        public string Id { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public SubtitleFormat Format { get; set; }
        public bool IsForced { get; set; }
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Media item type
    /// </summary>
    public enum MediaType
    {
        Unknown,
        Video,
        Audio,
        Photo
    }

    /// <summary>
    /// Subtitle file format
    /// </summary>
    public enum SubtitleFormat
    {
        Unknown,
        SRT,
        ASS,
        SSA,
        WEBVTT,
        VTT
    }

    /// <summary>
    /// Plugin configuration
    /// </summary>
    public class PluginConfiguration
    {
        /// <summary>
        /// Enable/disable automatic sync
        /// </summary>
        public bool EnableAutoSync { get; set; } = true;

        /// <summary>
        /// Minimum confidence threshold for auto-correction (0.0 to 1.0)
        /// </summary>
        public double MinConfidenceThreshold { get; set; } = 0.9;

        /// <summary>
        /// Maximum allowed offset before correction (milliseconds)
        /// </summary>
        public int MaxAllowedOffsetMs { get; set; } = 50;

        /// <summary>
        /// Subtitle formats to process
        /// </summary>
        public List<SubtitleFormat> EnabledFormats { get; set; } = new List<SubtitleFormat>
        {
            SubtitleFormat.SRT,
            SubtitleFormat.ASS,
            SubtitleFormat.WEBVTT
        };

        /// <summary>
        /// Create backups before modifying
        /// </summary>
        public bool CreateBackups { get; set; } = true;

        /// <summary>
        /// Notification settings
        /// </summary>
        public bool NotifyOnSync { get; set; } = true;

        /// <summary>
        /// Backup suffix for subtitle files
        /// </summary>
        public string BackupSuffix { get; set; } = ".syncbackup";
    }

    /// <summary>
    /// Logger interface
    /// </summary>
    public interface ILogger
    {
        void Debug(string message);
        void Debug(Exception exception, string message);
        void Info(string message);
        void Info(Exception exception, string message);
        void Warn(string message);
        void Warn(Exception exception, string message);
        void Error(string message);
        void Error(Exception exception, string message);
        void Fatal(string message);
        void Fatal(Exception exception, string message);
    }

    /// <summary>
    /// File system abstraction
    /// </summary>
    public interface IFileSystem
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        Task CopyFileAsync(string source, string destination, bool overwrite = false);
        Task<Stream> OpenReadAsync(string path);
        Task<Stream> OpenWriteAsync(string path);
        Task DeleteFileAsync(string path);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        string GetFileNameWithoutExtension(string path);
        string GetExtension(string path);
        string Combine(string path1, string path2);
        DateTime GetLastWriteTime(string path);
    }
}
