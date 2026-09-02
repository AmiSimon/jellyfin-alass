using System;
using SubtitleSync.Emby;
using SubtitleSync.Jellyfin;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Shared
{
    /// <summary>
    /// Factory for creating media server adapters based on the running server type.
    /// </summary>
    public static class ServerAdapterFactory
    {
        private static IMediaServerAbstraction? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets or creates the media server abstraction instance.
        /// </summary>
        public static IMediaServerAbstraction Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    if (_instance != null)
                        return _instance;

                    // Detect server type from compilation symbols
#if JELLYFIN
                    _instance = new JellyfinAdapter();
#elif EMBY
                    _instance = new EmbyAdapter();
#else
                    throw new InvalidOperationException("No media server type defined. Please compile with JELLYFIN or EMBY symbol.");
#endif

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Initializes the factory with a specific adapter instance.
        /// </summary>
        /// <param name="adapter">The adapter instance.</param>
        public static void Initialize(IMediaServerAbstraction adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter));

            lock (_lock)
            {
                _instance = adapter;
            }
        }

        /// <summary>
        /// Resets the factory instance.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Creates an adapter based on the server type name.
        /// </summary>
        /// <param name="serverType">The server type name (e.g., "Jellyfin", "Emby").</param>
        /// <returns>An IMediaServerAbstraction instance.</returns>
        public static IMediaServerAbstraction CreateAdapter(string serverType)
        {
            if (string.IsNullOrWhiteSpace(serverType))
                throw new ArgumentException("Server type cannot be null or empty.", nameof(serverType));

            return serverType switch
            {
                "Jellyfin" => new JellyfinAdapter(),
                "Emby" => new EmbyAdapter(),
                _ => throw new ArgumentException($"Unsupported server type: {serverType}")
            };
        }
    }
}
