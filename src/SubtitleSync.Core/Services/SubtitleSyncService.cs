using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SubtitleSync.Core.Interfaces;
using SubtitleSync.Core.Models;
using SubtitleSync.Shared.Interfaces;

namespace SubtitleSync.Core.Services
{
    /// <summary>
    /// Main service that orchestrates subtitle synchronization.
    /// </summary>
    public class SubtitleSyncService : IDisposable
    {
        private readonly IMediaServerAbstraction _server;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, bool> _processedItems = new ConcurrentDictionary<string, bool>();
        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private PluginConfiguration _configuration = new PluginConfiguration();

        /// <summary>
        /// Initializes a new instance of the SubtitleSyncService class.
        /// </summary>
        /// <param name="server">The media server abstraction.</param>
        public SubtitleSyncService(IMediaServerAbstraction server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _logger = server.GetLogger();
        }

        /// <summary>
        /// Initializes the service asynchronously.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _configuration = await _server.GetConfigurationAsync();
                _logger.Info("SubtitleSync service initialized");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize SubtitleSync service");
            }
        }

        /// <summary>
        /// Processes a media item and synchronizes its subtitles.
        /// </summary>
        /// <param name="itemId">The ID of the media item to process.</param>
        public async Task ProcessMediaItemAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return;

            // Check if already processed (idempotent operation)
            if (_processedItems.TryGetValue(itemId, out _))
            {
                _logger.Debug($