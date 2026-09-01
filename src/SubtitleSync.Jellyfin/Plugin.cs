using System;
using System.Collections.Generic;
using Jellyfin.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace SubtitleSync.Jellyfin
{
    /// <summary>
    /// Main plugin entry point for Jellyfin.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        public override string Name => "SubtitleSync";

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        public override string Description => "Automatically synchronizes out-of-sync subtitles for media files.";

        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public override Guid Id => Guid.Parse("A1B2C3D4-E5F6-7890-G1H2-I3J4K5L6M7N8");

        /// <summary>
        /// Gets the plugin category.
        /// </summary>
        public override string Category => "Subtitles";

        /// <summary>
        /// Gets the plugin target version.
        /// </summary>
        public override string TargetAbi => PluginTargetAbi.Jellyfin10_8_0;

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static Plugin Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Plugin class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        public Plugin(IApplicationPaths applicationPaths, IPluginManager pluginManager)
            : base(applicationPaths, pluginManager)
        {
            Instance = this;
        }

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public override void OnStartup()
        {
            base.OnStartup();

            // Register services
            // Note: In Jellyfin 10.8+, plugin services are registered differently
            // This would be handled through the plugin's service registrar
        }

        /// <summary>
        /// Called when the plugin is unloading.
        /// </summary>
        public override void OnShutdown()
        {
            base.OnShutdown();
        }

        /// <summary>
        /// Gets the web pages for the plugin configuration UI.
        /// </summary>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.config.html"
                }
            };
        }
    }

    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether auto sync is enabled.
        /// </summary>
        public bool EnableAutoSync { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum confidence threshold for auto-correction (0.0-1.0).
        /// </summary>
        public double MinConfidenceThreshold { get; set; } = 0.9;

        /// <summary>
        /// Gets or sets the maximum allowed offset in milliseconds.
        /// </summary>
        public int MaxAllowedOffsetMs { get; set; } = 50;

        /// <summary>
        /// Gets or sets the enabled subtitle formats.
        /// </summary>
        public List<string> EnabledFormats { get; set; } = new List<string> { "SRT", "ASS", "SSA", "WEBVTT" };

        /// <summary>
        /// Gets or sets a value indicating whether to create backups.
        /// </summary>
        public bool CreateBackups { get; set; } = true;

        /// <summary>
        /// Gets or sets the backup suffix.
        /// </summary>
        public string BackupSuffix { get; set; } = ".syncbackup";

        /// <summary>
        /// Gets or sets a value indicating whether to notify on sync.
        /// </summary>
        public bool NotifyOnSync { get; set; } = true;
    }
}
