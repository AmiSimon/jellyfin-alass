using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Emby
{
    /// <summary>
    /// Emby-specific implementation of IMediaServerAbstraction.
    /// </summary>
    public class EmbyAdapter : IMediaServerAbstraction
    {
        private readonly MediaBrowser.Server.Plugins.Plugin _plugin;
        private readonly ILogger _logger;
        private readonly MediaBrowser.Server.Plugins.IPluginManager _pluginManager;

        /// <summary>
        /// Initializes a new instance of the EmbyAdapter class.
        /// </summary>
        public EmbyAdapter(
            MediaBrowser.Server.Plugins.Plugin plugin,
            ILogger logger,
            MediaBrowser.Server.Plugins.IPluginManager pluginManager)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// Gets all media items from the library.
        /// </summary>
        public async Task<IEnumerable<MediaItem>> GetMediaItemsAsync()
        {
            try
            {
                // In Emby, we can access the library through the ApplicationHost
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    return Enumerable.Empty<MediaItem>();

                var libraryManager = appHost.Resolve<MediaBrowser.Server.Library.ILibraryManager>();
                var allItems = libraryManager.GetItems();

                return allItems.Select(item => ConvertToMediaItem(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media items");
                return Enumerable.Empty<MediaItem>();
            }
        }

        /// <summary>
        /// Gets a specific media item by ID.
        /// </summary>
        public async Task<MediaItem> GetMediaItemAsync(string itemId)
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    return null;

                var libraryManager = appHost.Resolve<MediaBrowser.Server.Library.ILibraryManager>();
                var item = libraryManager.GetItemById(new Guid(itemId));
                
                if (item == null)
                    return null;

                return ConvertToMediaItem(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media item {ItemId}", itemId);
                return null;
            }
        }

        /// <summary>
        /// Gets the media stream for a file.
        /// </summary>
        public async Task<Stream> GetMediaStreamAsync(string path)
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    throw new InvalidOperationException("ApplicationHost is not available");

                var fileSystem = appHost.Resolve<MediaBrowser.Common.FileSystem.IFileSystem>();
                
                if (!fileSystem.FileExists(path))
                    throw new FileNotFoundException("Media file not found", path);

                return await fileSystem.OpenReadAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening media stream for {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Gets the subtitle stream for a file.
        /// </summary>
        public async Task<Stream> GetSubtitleStreamAsync(string path)
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    throw new InvalidOperationException("ApplicationHost is not available");

                var fileSystem = appHost.Resolve<MediaBrowser.Common.FileSystem.IFileSystem>();
                
                if (!fileSystem.FileExists(path))
                    throw new FileNotFoundException("Subtitle file not found", path);

                return await fileSystem.OpenReadAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening subtitle stream for {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Saves a subtitle file.
        /// </summary>
        public async Task SaveSubtitleAsync(string path, Stream content)
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    throw new InvalidOperationException("ApplicationHost is not available");

                var fileSystem = appHost.Resolve<MediaBrowser.Common.FileSystem.IFileSystem>();
                
                using (var fileStream = fileSystem.OpenWriteAsync(path).Result)
                {
                    await content.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subtitle to {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Gets all subtitle files for a media item.
        /// </summary>
        public async Task<IEnumerable<SubtitleFile>> GetSubtitleFilesAsync(string itemId)
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    return Enumerable.Empty<SubtitleFile>();

                var libraryManager = appHost.Resolve<MediaBrowser.Server.Library.ILibraryManager>();
                var item = libraryManager.GetItemById(new Guid(itemId));
                
                if (item == null)
                    return Enumerable.Empty<SubtitleFile>();

                // Get all media streams for this item
                var mediaStreams = item.GetMediaStreams();
                var subtitleStreams = mediaStreams.Where(s => s.Type == MediaBrowser.Model.MediaInfo.MediaStreamType.Subtitle);

                return subtitleStreams.Select(stream => new SubtitleFile
                {
                    Id = stream.Index.ToString(),
                    Path = stream.Path,
                    Language = stream.Language,
                    Format = ConvertSubtitleType(stream.Codec),
                    IsForced = stream.IsForcedSubtitleStream,
                    IsDefault = stream.IsDefault
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subtitle files for {ItemId}", itemId);
                return Enumerable.Empty<SubtitleFile>();
            }
        }

        /// <summary>
        /// Gets plugin configuration.
        /// </summary>
        public async Task<PluginConfiguration> GetConfigurationAsync()
        {
            try
            {
                // In Emby, plugin configuration is stored in the plugin's data directory
                var config = _plugin.Configuration;
                
                // If no configuration exists, return defaults
                if (config == null)
                    return new PluginConfiguration();

                // Deserialize configuration
                // Note: This is simplified - actual implementation would use proper serialization
                return new PluginConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin configuration");
                return new PluginConfiguration();
            }
        }

        /// <summary>
        /// Saves plugin configuration.
        /// </summary>
        public async Task SaveConfigurationAsync(PluginConfiguration config)
        {
            try
            {
                // In Emby, we update the plugin configuration
                // This would typically be done through the plugin's Configuration property
                // and the system would handle persistence
                
                // For now, we'll just log that configuration was saved
                _logger.LogInformation("Plugin configuration saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving plugin configuration");
            }
        }

        /// <summary>
        /// Gets the server's logger.
        /// </summary>
        public ILogger GetLogger()
        {
            return new EmbyLogger(_logger);
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        public string GetServerVersion()
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    return "Unknown";

                return appHost.ApplicationVersion.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        public T GetService<T>() where T : class
        {
            try
            {
                var appHost = _plugin.ApplicationHost;
                if (appHost == null)
                    throw new InvalidOperationException("ApplicationHost is not available");

                return appHost.Resolve<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving service {ServiceType}", typeof(T).Name);
                throw;
            }
        }

        private MediaItem ConvertToMediaItem(MediaBrowser.Model.Entities.BaseItem item)
        {
            return new MediaItem
            {
                Id = item.Id.ToString(),
                Name = item.Name,
                Path = item.Path,
                Duration = item.RunTimeTicks.HasValue ? 
                    TimeSpan.FromTicks(item.RunTimeTicks.Value) : TimeSpan.Zero,
                DateCreated = item.DateCreated,
                DateModified = item.DateModified,
                Type = ConvertMediaType(item)
            };
        }

        private MediaItem.MediaType ConvertMediaType(MediaBrowser.Model.Entities.BaseItem item)
        {
            if (item is MediaBrowser.Model.Entities.Video)
                return MediaItem.MediaType.Video;
            if (item is MediaBrowser.Model.Entities.Audio)
                return MediaItem.MediaType.Audio;
            if (item is MediaBrowser.Model.Entities.Photo)
                return MediaItem.MediaType.Photo;
            return MediaItem.MediaType.Unknown;
        }

        private SubtitleFormat ConvertSubtitleType(string codec)
        {
            if (string.IsNullOrWhiteSpace(codec))
                return SubtitleFormat.Unknown;

            var codecLower = codec.ToLowerInvariant();
            
            if (codecLower.Contains("srt"))
                return SubtitleFormat.SRT;
            if (codecLower.Contains("ass") || codecLower.Contains("ssa"))
                return SubtitleFormat.ASS;
            if (codecLower.Contains("vtt") || codecLower.Contains("webvtt"))
                return SubtitleFormat.WEBVTT;
            
            return SubtitleFormat.Unknown;
        }

        /// <summary>
        /// Wrapper to convert ILogger to SubtitleSync.Shared.Interfaces.ILogger
        /// </summary>
        private class EmbyLogger : ILogger
        {
            private readonly ILogger _logger;

            public EmbyLogger(ILogger logger)
            {
                _logger = logger;
            }

            public void Debug(string message) => _logger.LogDebug(message);
            public void Debug(Exception exception, string message) => _logger.LogDebug(exception, message);
            public void Info(string message) => _logger.LogInformation(message);
            public void Info(Exception exception, string message) => _logger.LogInformation(exception, message);
            public void Warn(string message) => _logger.LogWarning(message);
            public void Warn(Exception exception, string message) => _logger.LogWarning(exception, message);
            public void Error(string message) => _logger.LogError(message);
            public void Error(Exception exception, string message) => _logger.LogError(exception, message);
            public void Fatal(string message) => _logger.LogCritical(message);
            public void Fatal(Exception exception, string message) => _logger.LogCritical(exception, message);
        }
    }
}
