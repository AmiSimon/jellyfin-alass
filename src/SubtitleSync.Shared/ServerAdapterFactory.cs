using System;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Shared
{
    /// <summary>
    /// Factory for creating the appropriate media server adapter based on runtime detection.
    /// </summary>
    public static class ServerAdapterFactory
    {
        private static IMediaServerAbstraction? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Creates and returns the appropriate adapter for the current media server.
        /// </summary>
        /// <returns>An instance of IMediaServerAbstraction</returns>
        /// <exception cref="InvalidOperationException">Thrown when the server type cannot be determined</exception>
        public static IMediaServerAbstraction Create()
        {
            lock (_lock)
            {
                if (_instance != null)
                    return _instance;

                // Try to detect Jellyfin
                if (IsJellyfin())
                {
                    _instance = CreateJellyfinAdapter();
                    return _instance;
                }

                // Try to detect Emby
                if (IsEmby())
                {
                    _instance = CreateEmbyAdapter();
                    return _instance;
                }

                throw new InvalidOperationException(
                    "Cannot determine media server type. " +
                    "Ensure this plugin is running within Jellyfin or Emby server.");
            }
        }

        /// <summary>
        /// Resets the factory instance (useful for testing)
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Sets a custom instance (useful for testing)
        /// </summary>
        public static void SetInstance(IMediaServerAbstraction instance)
        {
            lock (_lock)
            {
                _instance = instance;
            }
        }

        private static bool IsJellyfin()
        {
            try
            {
                // Check for Jellyfin-specific types
                var jellyfinDataType = Type.GetType("Jellyfin.Data.Plugins.Plugin, Jellyfin.Data");
                var jellyfinModelType = Type.GetType("MediaBrowser.Model.Plugins.PluginInfo, MediaBrowser.Model");
                
                // Additional check for Jellyfin's server type
                var serverType = Type.GetType("Jellyfin.Server.JellyfinServer, Jellyfin.Server");
                
                return jellyfinDataType != null || serverType != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsEmby()
        {
            try
            {
                // Check for Emby-specific types
                var embyServerType = Type.GetType("MediaBrowser.Server.Plugins.Plugin, Emby.Server");
                var embyModelType = Type.GetType("MediaBrowser.Model.Plugins.PluginInfo, MediaBrowser.Model");
                
                // Additional check for Emby's server type
                var serverType = Type.GetType("Emby.Server.EmbyServer, Emby.Server");
                
                return embyServerType != null || serverType != null;
            }
            catch
            {
                return false;
            }
        }

        private static IMediaServerAbstraction CreateJellyfinAdapter()
        {
#if JELLYFIN
            return new JellyfinAdapter();
#else
            throw new InvalidOperationException(
                "Jellyfin adapter cannot be created because this assembly was not compiled for Jellyfin.");
#endif
        }

        private static IMediaServerAbstraction CreateEmbyAdapter()
        {
#if EMBY
            return new EmbyAdapter();
#else
            throw new InvalidOperationException(
                "Emby adapter cannot be created because this assembly was not compiled for Emby.");
#endif
        }
    }
}
