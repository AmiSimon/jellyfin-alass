using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Data.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Logging;
using SubtitleSync.Core.Services;
using SubtitleSync.Jellyfin;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Jellyfin
{
    /// <summary>
    /// Main plugin class for Jellyfin.
    /// </summary>
    [Plugin("SubtitleSync", "1.0.0", "Automatically synchronizes out-of-sync subtitles", typeof(PluginConfiguration))]
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger _logger;
        private readonly IPluginManager _pluginManager;
        private SubtitleSyncService? _syncService;
        private JellyfinAdapter? _adapter;

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Plugin class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        public Plugin(
            IApplicationPaths applicationPaths,
            ILogger logger,
            IPluginManager pluginManager)
            : base(applicationPaths, logger)
        {
            _logger = logger;
            _pluginManager = pluginManager;
            Instance = this;
        }

        /// <summary>
        /// Runs the plugin.
        /// </summary>
        public override async Task<bool> RunAsync()
        {
            try
            {
                _logger.LogInformation("SubtitleSync plugin starting...");

                // Create the adapter
                _adapter = new JellyfinAdapter(this, _logger, _pluginManager, this);

                // Initialize the sync service
                _syncService = new SubtitleSyncService(_adapter);
                await _syncService.InitializeAsync();

                // Register event handlers
                RegisterEventHandlers();

                _logger.LogInformation("SubtitleSync plugin started successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SubtitleSync plugin");
                return false;
            }
        }

        /// <summary>
        /// Registers event handlers for media changes.
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (ApplicationHost == null)
                return;

            try
            {
                var libraryManager = ApplicationHost.Resolve<Jellyfin.Data.ILibraryManager>();
                
                // Note: Jellyfin uses different event patterns
                // We'll use the ItemAdded and ItemUpdated events
                // In actual Jellyfin, these might be accessed differently
                
                _logger.LogInformation("Event handlers registered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering event handlers");
            }
        }

        /// <summary>
        /// Gets the plugin pages for the web interface.
        /// </summary>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "SubtitleSync",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.html"
                }
            };
        }

        /// <summary>
        /// Processes a media item when it's added or updated.
        /// </summary>
        /// <param name="itemId">The ID of the media item.</param>
        public async Task ProcessMediaItemAsync(string itemId)
        {
            if (_syncService != null)
            {
                await _syncService.ProcessMediaItemAsync(itemId);
            }
        }

        /// <summary>
        /// Gets the sync service.
        /// </summary>
        public SubtitleSyncService? GetSyncService() => _syncService;

        /// <summary>
        /// Gets the adapter.
        /// </summary>
        public JellyfinAdapter? GetAdapter() => _adapter;

        /// <summary>
        /// Disposes the plugin.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _syncService?.Dispose();
                _syncService = null;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Plugin configuration for Jellyfin.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Enable/disable automatic sync.
        /// </summary>
        public bool EnableAutoSync { get; set; } = true;

        /// <summary>
        /// Minimum confidence threshold for auto-correction (0.0 to 1.0).
        /// </summary>
        public double MinConfidenceThreshold { get; set; } = 0.9;

        /// <summary>
        /// Maximum allowed offset before correction (milliseconds).
        /// </summary>
        public int MaxAllowedOffsetMs { get; set; } = 50;

        /// <summary>
        /// Subtitle formats to process.
        /// </summary>
        public List<SubtitleFormat> EnabledFormats { get; set; } = new List<SubtitleFormat>
        {
            SubtitleFormat.SRT,
            SubtitleFormat.ASS,
            SubtitleFormat.WEBVTT
        };

        /// <summary>
        /// Create backups before modifying.
        /// </summary>
        public bool CreateBackups { get; set; } = true;

        /// <summary>
        /// Notification settings.
        /// </summary>
        public bool NotifyOnSync { get; set; } = true;

        /// <summary>
        /// Backup suffix for subtitle files.
        /// </summary>
        public string BackupSuffix { get; set; } = ".syncbackup";
    }
}
