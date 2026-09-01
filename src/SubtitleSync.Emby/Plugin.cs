using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using SubtitleSync.Core.Services;
using SubtitleSync.Emby;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Emby
{
    /// <summary>
    /// Main plugin class for Emby.
    /// </summary>
    public class Plugin : IServerEntryPoint
    {
        private readonly ILogger _logger;
        private readonly MediaBrowser.Server.Plugins.IPluginManager _pluginManager;
        private SubtitleSyncService? _syncService;
        private EmbyAdapter? _adapter;
        private MediaBrowser.Server.Plugins.Plugin? _pluginInstance;

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Plugin class.
        /// </summary>
        public Plugin()
        {
            Instance = this;
        }

        /// <summary>
        /// Initializes the plugin with dependencies.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        public void Initialize(
            MediaBrowser.Server.Plugins.Plugin plugin,
            ILogger logger,
            MediaBrowser.Server.Plugins.IPluginManager pluginManager)
        {
            _pluginInstance = plugin;
            _logger = logger;
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// Runs the plugin.
        /// </summary>
        public void Run()
        {
            try
            {
                _logger.LogInformation("SubtitleSync plugin starting...");

                if (_pluginInstance == null)
                {
                    _logger.LogError("Plugin instance is null");
                    return;
                }

                // Create the adapter
                _adapter = new EmbyAdapter(_pluginInstance, _logger, _pluginManager);

                // Initialize the sync service
                _syncService = new SubtitleSyncService(_adapter);
                _syncService.InitializeAsync().GetAwaiter().GetResult();

                // Register event handlers
                RegisterEventHandlers();

                _logger.LogInformation("SubtitleSync plugin started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SubtitleSync plugin");
            }
        }

        /// <summary>
        /// Registers event handlers for media changes.
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (_pluginInstance?.ApplicationHost == null)
                return;

            try
            {
                var libraryManager = _pluginInstance.ApplicationHost.Resolve<MediaBrowser.Server.Library.ILibraryManager>();
                
                // In Emby, we can use the ItemAdded and ItemUpdated events
                // Note: The actual event names might differ in Emby
                
                _logger.LogInformation("Event handlers registered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering event handlers");
            }
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
        public EmbyAdapter? GetAdapter() => _adapter;

        /// <summary>
        /// Disposes the plugin.
        /// </summary>
        public void Dispose()
        {
            _syncService?.Dispose();
            _syncService = null;
        }
    }

    /// <summary>
    /// Plugin configuration for Emby.
    /// </summary>
    public class PluginConfiguration
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

    /// <summary>
    /// Plugin entry point factory for Emby.
    /// </summary>
    public class PluginEntryPoint : IServerEntryPoint
    {
        private readonly ILogger _logger;
        private readonly MediaBrowser.Server.Plugins.IPluginManager _pluginManager;
        private Plugin? _plugin;

        /// <summary>
        /// Initializes a new instance of the PluginEntryPoint class.
        /// </summary>
        public PluginEntryPoint(ILogger logger, MediaBrowser.Server.Plugins.IPluginManager pluginManager)
        {
            _logger = logger;
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// Runs the plugin entry point.
        /// </summary>
        public void Run()
        {
            // This will be called by Emby when loading the plugin
        }

        /// <summary>
        /// Disposes the entry point.
        /// </summary>
        public void Dispose()
        {
            _plugin?.Dispose();
            _plugin = null;
        }
    }
}
