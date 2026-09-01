# Testing Guide for SubtitleSync Plugin

This guide provides detailed instructions on how to test the SubtitleSync plugin with both Jellyfin and Emby.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Test Scenarios](#test-scenarios)
4. [Manual Testing](#manual-testing)
5. [Automated Testing](#automated-testing)
6. [Debugging](#debugging)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Software Requirements

- **Jellyfin 10.8+** or **Emby 4.7+**
- **.NET 8.0 SDK** (for building from source)
- **Git** (for cloning the repository)
- **Text editor** (VS Code, Notepad++, etc.)

### Hardware Requirements

- Minimum 2GB RAM
- 1GB free disk space
- At least one CPU core

---

## Quick Start

### For Jellyfin Users

```bash
# Clone the repository
cd /workspace

# Build the Jellyfin plugin
dotnet build SubtitleSyncPlugin/src/SubtitleSync.Jellyfin/SubtitleSync.Jellyfin.csproj -c Release

# Copy to plugins directory (Linux example)
mkdir -p /var/lib/jellyfin/plugins/SubtitleSync
cp SubtitleSyncPlugin/src/SubtitleSync.Jellyfin/bin/Release/net8.0/SubtitleSync.Jellyfin.dll /var/lib/jellyfin/plugins/SubtitleSync/
cp SubtitleSyncPlugin/src/SubtitleSync.Jellyfin/plugin.json /var/lib/jellyfin/plugins/SubtitleSync/

# Restart Jellyfin
sudo systemctl restart jellyfin
```

### For Emby Users

```bash
# Clone the repository
cd /workspace

# Build the Emby plugin
dotnet build SubtitleSyncPlugin/src/SubtitleSync.Emby/SubtitleSync.Emby.csproj -c Release

# Copy to plugins directory (Linux example)
mkdir -p /var/lib/emby-server/plugins/SubtitleSync
cp SubtitleSyncPlugin/src/SubtitleSync.Emby/bin/Release/net8.0/SubtitleSync.Emby.dll /var/lib/emby-server/plugins/SubtitleSync/
cp SubtitleSyncPlugin/src/SubtitleSync.Emby/manifest.json /var/lib/emby-server/plugins/SubtitleSync/

# Restart Emby
sudo systemctl restart emby-server
```

---

## Test Scenarios

### Scenario 1: Basic Sync Detection

**Objective:** Verify that the plugin detects out-of-sync subtitles.

**Steps:**

1. Create a test media file (e.g., `test.mp4`)
2. Create an SRT file with subtitles that start too late:
   ```
   1
   00:00:10,000 --> 00:00:13,000
   This subtitle starts 10 seconds late
   
   2
   00:00:15,000 --> 00:00:18,000
   Another subtitle
   ```
3. Add both files to your library
4. Check the plugin logs for sync detection messages

**Expected Result:**
- Plugin detects subtitles are out of sync
- Logs show the detected offset

---

### Scenario 2: Automatic Correction

**Objective:** Verify that the plugin automatically corrects out-of-sync subtitles.

**Steps:**

1. Enable "Enable Auto Sync" in plugin configuration
2. Set "Min Confidence Threshold" to 0.5 (lower for testing)
3. Set "Max Allowed Offset (ms)" to 1000 (1 second)
4. Add a media file with out-of-sync subtitles (offset > 1 second)
5. Wait for automatic processing or click "Test Sync Now"

**Expected Result:**
- Subtitles are corrected
- Backup file is created (if enabled)
- Logs show successful sync

---

### Scenario 3: Backup System

**Objective:** Verify that backups are created before modification.

**Steps:**

1. Enable "Create Backups" in configuration
2. Add a media file with out-of-sync subtitles
3. Wait for processing
4. Check the media directory for backup files

**Expected Result:**
- Original subtitle file is backed up with `.syncbackup` suffix
- Corrected subtitle file replaces the original

---

### Scenario 4: Multiple Formats

**Objective:** Verify that the plugin processes different subtitle formats.

**Steps:**

1. Enable all formats in configuration (SRT, ASS, WEBVTT)
2. Create test files:
   - `subtitles.srt` - SubRip format
   - `subtitles.ass` - Advanced SubStation Alpha format
   - `subtitles.vtt` - WebVTT format
3. Add media with all three subtitle files
4. Check logs for processing of each format

**Expected Result:**
- All enabled formats are processed
- Each format is correctly parsed and corrected

---

### Scenario 5: Idempotent Processing

**Objective:** Verify that each file is processed only once.

**Steps:**

1. Add a media file with out-of-sync subtitles
2. Wait for processing to complete
3. Modify the media file (trigger update)
4. Check logs

**Expected Result:**
- First processing: subtitles are corrected
- Second processing: file is skipped (already processed)

---

### Scenario 6: Configuration Changes

**Objective:** Verify that configuration changes take effect.

**Steps:**

1. Set "Enable Auto Sync" to false
2. Add a media file with out-of-sync subtitles
3. Verify no processing occurs
4. Enable "Enable Auto Sync"
5. Click "Test Sync Now"

**Expected Result:**
- No processing when disabled
- Processing occurs after enabling and clicking "Test Sync Now"

---

### Scenario 7: Error Handling

**Objective:** Verify that the plugin handles errors gracefully.

**Steps:**

1. Create a corrupted subtitle file
2. Add it to a media item
3. Check logs for error messages
4. Verify the original file is not modified

**Expected Result:**
- Error is logged
- Original file is not modified
- If backups exist, they are restored

---

## Manual Testing

### Using the Configuration Page

1. Open your Jellyfin/Emby dashboard
2. Navigate to Plugins
3. Find "SubtitleSync" and click "Configure"
4. Adjust settings as needed
5. Click "Test Sync Now" to force processing
6. View the log at the bottom of the page

### Using Server Logs

**Jellyfin:**
```bash
# View logs
journalctl -u jellyfin -f

# Filter for SubtitleSync
journalctl -u jellyfin -f | grep -i subtitle
```

**Emby:**
```bash
# View logs
journalctl -u emby-server -f

# Filter for SubtitleSync
journalctl -u emby-server -f | grep -i subtitle
```

### Testing with Real Media

1. Add a movie or TV show to your library
2. Ensure it has subtitle files
3. Intentionally modify the subtitle timing to be out of sync
4. Wait for automatic processing or trigger manually
5. Verify the subtitles are corrected

---

## Automated Testing

### Running Unit Tests

```bash
# Navigate to the tests directory
cd /workspace/SubtitleSyncPlugin

# Run all tests
dotnet test tests/SubtitleSync.Core.Tests/SubtitleSync.Core.Tests.csproj

# Run with verbose output
dotnet test tests/SubtitleSync.Core.Tests/SubtitleSync.Core.Tests.csproj -v n
```

### Test Cases Included

The test suite includes tests for:

- **Subtitle Parsing:**
  - SRT format parsing and writing
  - ASS format parsing and writing
  - WebVTT format parsing and writing
  - Error handling for malformed files

- **Sync Detection:**
  - First subtitle detection
  - Content matching (simplified)
  - Offset calculation

- **Sync Correction:**
  - Offset application
  - Overlap fixing
  - Validation

- **Backup System:**
  - Backup creation
  - Backup restoration
  - Backup validation

### Adding New Tests

To add a new test:

1. Create a new test class in `tests/SubtitleSync.Core.Tests/`
2. Add the `[TestFixture]` attribute (for NUnit)
3. Add test methods with the `[Test]` attribute
4. Run the tests

Example:
```csharp
using NUnit.Framework;
using SubtitleSync.Core.SubtitleParsers;
using System.IO;
using System.Threading.Tasks;

[TestFixture]
public class SrtParserTests
{
    [Test]
    public async Task Parse_SimpleSrt_ReturnsCorrectEntries()
    {
        var parser = new SrtParser();
        var content = "1\n00:00:01,000 --> 00:00:03,000\nTest subtitle\n\n2\n00:00:05,000 --> 00:00:07,000\nAnother subtitle\n";
        
        var entries = await parser.ParseAsync(content);
        
        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("Test subtitle", entries[0].Text);
    }
}
```

---

## Debugging

### Debugging in Visual Studio

1. Open the solution file: `SubtitleSyncPlugin/SubtitleSync.sln`
2. Set the startup project to `SubtitleSync.Jellyfin` or `SubtitleSync.Emby`
3. Configure debugging:
   - For Jellyfin: Set the working directory to your Jellyfin server
   - For Emby: Set the working directory to your Emby server
4. Set breakpoints in the code
5. Start debugging

### Debugging with dotnet CLI

```bash
# Build with debug symbols
dotnet build SubtitleSync.Jellyfin/SubtitleSync.Jellyfin.csproj -c Debug

# Attach to running process (Linux)
dotnet-dump collect --process-id <pid>
dotnet-dump analyze <dump-file>
```

### Logging

The plugin uses the server's logging system. Log levels:

- **Debug:** Detailed information for troubleshooting
- **Info:** General information about operations
- **Warn:** Warning messages (non-critical issues)
- **Error:** Error messages (failed operations)
- **Fatal:** Critical errors (plugin may not function)

To increase log verbosity:
- In Jellyfin/Emby configuration, set log level to "Debug"
- Restart the server

---

## Troubleshooting

### Plugin Not Loading

**Symptoms:**
- Plugin doesn't appear in the dashboard
- No errors in logs

**Solutions:**

1. **Check file locations:**
   ```bash
   # Jellyfin
   ls -la /var/lib/jellyfin/plugins/SubtitleSync/
   
   # Emby
   ls -la /var/lib/emby-server/plugins/SubtitleSync/
   ```

2. **Check file permissions:**
   ```bash
   # Jellyfin
   chmod -R 755 /var/lib/jellyfin/plugins/SubtitleSync
   chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/SubtitleSync
   
   # Emby
   chmod -R 755 /var/lib/emby-server/plugins/SubtitleSync
   chown -R emby:emby /var/lib/emby-server/plugins/SubtitleSync
   ```

3. **Check .NET version:**
   ```bash
   dotnet --version
   ```
   Ensure it's 8.0 or later.

4. **Check manifest file:**
   - Verify `plugin.json` (Jellyfin) or `manifest.json` (Emby) is valid JSON
   - Check the `targetAbi` matches your server version

---

### Sync Not Working

**Symptoms:**
- Subtitles remain out of sync
- No errors in logs

**Solutions:**

1. **Check configuration:**
   - Ensure "Enable Auto Sync" is checked
   - Verify the subtitle format is enabled
   - Check confidence threshold and offset settings

2. **Check file permissions:**
   ```bash
   # Ensure server user can read/write subtitle files
   chmod -R 755 /path/to/media
   chown -R jellyfin:jellyfin /path/to/media  # or emby:emby
   ```

3. **Force reprocessing:**
   - Click "Test Sync Now" in the configuration page
   - Or restart the server

4. **Check file extensions:**
   - Ensure subtitle files have correct extensions (.srt, .ass, .vtt)
   - Some servers may not recognize .webvtt (use .vtt instead)

---

### Backups Not Created

**Symptoms:**
- "Create Backups" is enabled
- No backup files are created

**Solutions:**

1. **Check permissions:**
   ```bash
   # Ensure server user can write to media directory
   touch /path/to/media/test.txt
   chown jellyfin:jellyfin /path/to/media/test.txt
   rm /path/to/media/test.txt
   ```

2. **Check disk space:**
   ```bash
   df -h /path/to/media
   ```

3. **Check configuration:**
   - Verify "Backup Suffix" doesn't contain invalid characters
   - Try a simpler suffix like `.bak`

---

### Errors in Logs

**Common Errors:**

1. **"File not found":**
   - Check the file path in the error message
   - Verify the file exists
   - Check file permissions

2. **"Access denied":**
   - File permission issue
   - Run `chmod` and `chown` to fix permissions

3. **"Invalid format":**
   - Subtitle file may be corrupted
   - Try opening the file in a text editor
   - Check for encoding issues (UTF-8 vs other encodings)

4. **"Out of memory":**
   - Very large subtitle files may cause issues
   - Try with smaller files first

---

## Performance Testing

### Test with Large Libraries

1. Add 100+ media items with subtitles
2. Click "Test Sync Now"
3. Monitor server resource usage:
   ```bash
   # CPU usage
   top
   
   # Memory usage
   free -h
   
   # Disk I/O
   iostat -x 1
   ```

4. Check how long processing takes
5. Verify all files are processed correctly

### Stress Testing

1. Add 1000+ media items
2. Enable all subtitle formats
3. Set low confidence threshold (0.1)
4. Click "Test Sync Now"
5. Monitor for:
   - Memory leaks
   - CPU spikes
   - Timeouts
   - Errors

---

## Test Data

### Sample SRT File (Out of Sync)

```
1
00:00:10,000 --> 00:00:13,000
This is the first subtitle

2
00:00:15,000 --> 00:00:18,000
This is the second subtitle

3
00:00:20,000 --> 00:00:23,000
This is the third subtitle
```

### Sample ASS File (Out of Sync)

```
[Script Info]
Title: Test Subtitles

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,2,0,0,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:10.00,0:00:13.00,Default,,0,0,0,,This is the first subtitle
Dialogue: 0,0:00:15.00,0:00:18.00,Default,,0,0,0,,This is the second subtitle
```

### Sample WebVTT File (Out of Sync)

```
WEBVTT

00:00:10.000 --> 00:00:13.000
This is the first subtitle

00:00:15.000 --> 00:00:18.000
This is the second subtitle
```

---

## Reporting Issues

When reporting issues, please include:

1. **Server version:** Jellyfin 10.8.x or Emby 4.7.x
2. **Plugin version:** 1.0.0
3. **Operating system:** Linux/Windows/macOS
4. **Steps to reproduce:** Detailed steps
5. **Expected behavior:** What should happen
6. **Actual behavior:** What actually happens
7. **Log files:** Relevant log entries
8. **Test data:** Sample files if applicable

---

## Success Criteria

The plugin is considered working correctly if:

- ✅ Plugin loads without errors
- ✅ Configuration page is accessible
- ✅ Settings are saved and loaded correctly
- ✅ Subtitles are detected as out of sync (when they are)
- ✅ Subtitles are corrected automatically (when enabled)
- ✅ Backups are created (when enabled)
- ✅ No data loss occurs
- ✅ Performance is acceptable for large libraries
- ✅ Plugin handles errors gracefully

---

## Final Notes

- The plugin is designed to be **safe** - it won't modify files without backups (if enabled)
- The plugin is **idempotent** - it won't reprocess files unnecessarily
- The plugin is **configurable** - you can adjust settings to match your needs
- The plugin is **cross-platform** - works with both Jellyfin and Emby

For best results:
- Start with a small test library
- Verify basic functionality works
- Gradually increase the library size
- Monitor performance and resource usage
- Report any issues with detailed information
