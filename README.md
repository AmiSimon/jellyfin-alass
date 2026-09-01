# SubtitleSync Plugin for Jellyfin and Emby

A server-side plugin that automatically detects and corrects out-of-sync subtitles for media files in **Jellyfin** and **Emby**. The synchronization runs **once per media file** (idempotent) and uses a **compatibility abstraction layer** to support both platforms from a single codebase.

## Features

- ✅ **Automatic Sync Detection**: Detects when subtitles are out of sync with the media
- ✅ **Multiple Detection Methods**: First subtitle, content matching, scene detection
- ✅ **Multiple Formats**: Supports SRT, ASS/SSA, WebVTT subtitle formats
- ✅ **Idempotent Processing**: Each file is processed only once
- ✅ **Backup System**: Creates backups of original subtitle files before modification
- ✅ **Configurable**: Adjust sync thresholds, enabled formats, and more
- ✅ **Cross-Platform**: Works with both **Jellyfin 10.8+** and **Emby 4.7+**
- ✅ **Web UI**: Configuration page accessible from the server dashboard

## Architecture

The plugin uses a layered architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    SUBTITLE SYNC PLUGIN                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Jellyfin    │  │   Emby      │  │   Shared Core        │ │
│  │ Adapter     │  │ Adapter     │  │ (Subtitle Processing)│ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
│           ↑                ↑  -  IMediaServerAbstraction        │
│           └─────────────────────────────────────────────────┘ │
│                        Compatibility Layer                      │
└─────────────────────────────────────────────────────────────┘
```

The **Compatibility Abstraction Layer** (`IMediaServerAbstraction`) allows the same core logic to work with both Jellyfin and Emby by abstracting platform-specific differences.

## Project Structure

```
/SubtitleSyncPlugin
├── /src
│   ├── /SubtitleSync.Core          # Shared business logic (PCL)
│   │   ├── /Models                 # Data models
│   │   ├── /Interfaces             # Interfaces
│   │   ├── /SubtitleParsers        # SRT, ASS, WebVTT parsers
│   │   └── /Services               # Sync detection, correction, backup
│   │
│   ├── /SubtitleSync.Shared        # Shared adapter interfaces
│   │   └── /Interfaces
│   │
│   ├── /SubtitleSync.Jellyfin      # Jellyfin-specific implementation
│   │   ├── Plugin.cs               # Main plugin entry point
│   │   ├── JellyfinAdapter.cs      # Jellyfin adapter
│   │   ├── plugin.json             # Jellyfin manifest
│   │   └── /Configuration          # Web UI files
│   │
│   └── /SubtitleSync.Emby          # Emby-specific implementation
│       ├── Plugin.cs               # Main plugin entry point
│       ├── EmbyAdapter.cs          # Emby adapter
│       └── manifest.json           # Emby manifest
│
├── /tests
│   └── /SubtitleSync.Core.Tests    # Unit tests
│
├── SubtitleSync.sln               # Visual Studio solution
└── README.md                       # This file
```

## Installation

### For Jellyfin

1. **Build the plugin:**
   ```bash
   cd /workspace/SubtitleSyncPlugin
   dotnet build SubtitleSync.Jellyfin/SubtitleSync.Jellyfin.csproj -c Release
   ```

2. **Copy the plugin:**
   - Locate the built DLL: `src/SubtitleSync.Jellyfin/bin/Release/net8.0/SubtitleSync.Jellyfin.dll`
   - Copy it to your Jellyfin plugins directory:
     - **Linux**: `/var/lib/jellyfin/plugins/SubtitleSync/`
     - **Windows**: `%AppData%\jellyfin\plugins\SubtitleSync\`
   - Also copy the `plugin.json` file to the same directory

3. **Restart Jellyfin:**
   ```bash
   sudo systemctl restart jellyfin
   # or
   sudo service jellyfin restart
   ```

4. **Configure the plugin:**
   - Open Jellyfin dashboard
   - Go to **Plugins** > **SubtitleSync**
   - Click **Configure** to adjust settings

### For Emby

1. **Build the plugin:**
   ```bash
   cd /workspace/SubtitleSyncPlugin
   dotnet build SubtitleSync.Emby/SubtitleSync.Emby.csproj -c Release
   ```

2. **Copy the plugin:**
   - Locate the built DLL: `src/SubtitleSync.Emby/bin/Release/net8.0/SubtitleSync.Emby.dll`
   - Copy it to your Emby plugins directory:
     - **Linux**: `/var/lib/emby-server/plugins/SubtitleSync/`
     - **Windows**: `%ProgramData%\Emby-Server\plugins\SubtitleSync\`
   - Also copy the `manifest.json` file to the same directory

3. **Restart Emby:**
   ```bash
   sudo systemctl restart emby-server
   # or
   sudo service emby-server restart
   ```

4. **Configure the plugin:**
   - Open Emby dashboard
   - Go to **Plugins** > **SubtitleSync**
   - Click **Configure** to adjust settings

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| **Enable Auto Sync** | Enable/disable automatic synchronization | `true` |
| **Min Confidence Threshold** | Minimum confidence for auto-correction (0.0-1.0) | `0.9` |
| **Max Allowed Offset (ms)** | Only correct if offset exceeds this value | `50` |
| **Enabled Formats** | Which subtitle formats to process | `SRT, ASS, WEBVTT` |
| **Create Backups** | Backup original files before modifying | `true` |
| **Backup Suffix** | Suffix for backup files | `.syncbackup` |
| **Notify on Sync** | Send notifications when syncing | `true` |

## Testing the Plugin

### Method 1: Manual Testing with Sample Files

1. **Create test media and subtitles:**
   ```bash
   # Create a test directory
   mkdir -p ~/test_media
   cd ~/test_media
   
   # Create a simple video file (or use an existing one)
   # For testing, you can use a short video file
   
   # Create an out-of-sync SRT file (subtitles.srt)
   cat > subtitles.srt << 'EOF'
1
00:00:05,000 --> 00:00:08,000
This subtitle is 5 seconds out of sync

2
00:00:10,000 --> 00:00:13,000
This is another test subtitle
EOF
   ```

2. **Add to your library:**
   - Add the test media to your Jellyfin/Emby library
   - Ensure the subtitle file is recognized

3. **Trigger processing:**
   - The plugin should automatically process new media
   - Or manually trigger by editing the media item

4. **Check results:**
   - Verify that subtitles were corrected
   - Check the backup file was created: `subtitles.srt.syncbackup`
   - Review the plugin logs for details

### Method 2: Using the Test Sync Button

1. Open the plugin configuration page
2. Click **"Test Sync Now"** button
3. Check the log for processing results
4. Verify subtitles were synchronized

### Method 3: Programmatic Testing

You can test the core subtitle processing logic directly:

```csharp
// Example: Test subtitle parsing and correction
using SubtitleSync.Core.Models;
using SubtitleSync.Core.SubtitleParsers;
using SubtitleSync.Core.Services;
using System.IO;

// Create a test SRT file with out-of-sync subtitles
var srtContent = "1\n00:00:05,000 --> 00:00:08,000\nTest subtitle\n\n2\n00:00:10,000 --> 00:00:13,000\nAnother subtitle\n";

// Parse the SRT
var parser = new SrtParser();
var entries = await parser.ParseAsync(srtContent);

// Create a sync detector (with a mock logger)
var detector = new SyncDetector(parser, new MockLogger());

// Detect sync issues
var result = await detector.DetectAsync(
    new MemoryStream(System.Text.Encoding.UTF8.GetBytes(srtContent)),
    TimeSpan.FromSeconds(30), // Media duration
    SyncDetectionMethod.FirstSubtitle);

// Apply correction if needed
if (result.IsOutOfSync)
{
    var corrector = new SyncCorrector(parser, new MockLogger());
    var correctedStream = await corrector.CorrectAsync(
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(srtContent)),
        result.Offset);
    
    // Read corrected content
    correctedStream.Position = 0;
    using var reader = new StreamReader(correctedStream);
    var correctedContent = await reader.ReadToEndAsync();
    Console.WriteLine("Corrected SRT:");
    Console.WriteLine(correctedContent);
}
```

## Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (optional, for GUI development)
- Git

### Build Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-repo/SubtitleSyncPlugin.git
   cd SubtitleSyncPlugin
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore SubtitleSync.sln
   ```

3. **Build the solution:**
   ```bash
   dotnet build SubtitleSync.sln -c Release
   ```

4. **Build specific platform:**
   ```bash
   # For Jellyfin
   dotnet build src/SubtitleSync.Jellyfin/SubtitleSync.Jellyfin.csproj -c Release
   
   # For Emby
   dotnet build src/SubtitleSync.Emby/SubtitleSync.Emby.csproj -c Release
   ```

5. **Run tests:**
   ```bash
   dotnet test tests/SubtitleSync.Core.Tests/SubtitleSync.Core.Tests.csproj
   ```

## Troubleshooting

### Plugin not appearing in dashboard

1. **Check plugin directory:**
   - Ensure the DLL is in the correct plugins directory
   - Verify the manifest file (plugin.json/manifest.json) is present

2. **Check file permissions:**
   ```bash
   # Linux
   chmod -R 755 /var/lib/jellyfin/plugins/SubtitleSync
   chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/SubtitleSync
   ```

3. **Restart the server:**
   - Sometimes a restart is needed for new plugins to be detected

4. **Check server logs:**
   ```bash
   # Jellyfin
   journalctl -u jellyfin -f
   
   # Emby
   journalctl -u emby-server -f
   ```

### Sync not working

1. **Check configuration:**
   - Ensure "Enable Auto Sync" is checked
   - Verify the subtitle format is in the enabled list

2. **Check plugin logs:**
   - View the plugin's log output in the configuration page

3. **Test manually:**
   - Click "Test Sync Now" to force processing

4. **Check file permissions:**
   - Ensure the Jellyfin/Emby user has write permissions to subtitle files

### Backups not being created

1. **Verify setting:**
   - Ensure "Create Backups" is enabled

2. **Check permissions:**
   - The server user needs write permissions in the media directory

3. **Check disk space:**
   - Ensure there's enough disk space for backups

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

## License

This project is licensed under the **MIT License**.

## Acknowledgments

- [Jellyfin](https://jellyfin.org/) - The free media system
- [Emby](https://emby.media/) - The media server this was originally forked from
- All contributors and testers

---

**Note:** This plugin is designed to be **idempotent** - it will only process each media file once. If you need to reprocess files, you can:
- Reset the plugin state through the configuration
- Manually delete the plugin's data to force reprocessing
- Use the "Test Sync Now" button to force a full rescan
