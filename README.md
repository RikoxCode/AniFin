# AniFin - Jellyfin Anime Downloader Plugin

<p align="center">
  <img src="https://img.shields.io/github/v/release/RikoxCode/AniFin" alt="Release">
  <img src="https://img.shields.io/github/license/RikoxCode/AniFin" alt="License">
  <img src="https://img.shields.io/github/issues/RikoxCode/AniFin" alt="Issues">
  <img src="https://img.shields.io/github/stars/RikoxCode/AniFin" alt="Stars">
</p>

AniFin is a powerful Jellyfin plugin that allows you to download anime series from AniWorld directly into your Jellyfin media library with automatic organization and queue management.

## ✨ Features

### 🎬 Download Management
- Download complete anime series, individual seasons, or specific episodes
- Optional automatic download of specials and movies
- Smart download queue with real-time progress tracking
- Pause, resume, and cancel downloads
- Automatic retry on failed downloads

### ⚙️ Configuration
- Multi-language support (German Dub, German Sub, English Sub)
- Multiple video providers (VOE, Vidoza, Streamtape, Doodstream)
- Customizable download paths
- Configurable concurrent downloads (1-10)

### 🖥️ Modern Web Interface
- Clean, intuitive tabbed interface
- Real-time statistics dashboard
- Comprehensive download history
- Auto-refreshing queue status

### 🌐 Browser Extension
- Chrome/Edge/Brave extension for one-click downloads
- Direct integration with AniWorld
- Easy configuration

### 📁 Smart File Organization
- Automatic organization into Jellyfin-compatible structure
- Format: `Anime Name/Season X/Anime Name - SxxExx - (Language).mp4`
- Separate folders for specials and movies
- Automatic cleanup of empty directories

## 📦 Installation

### Quick Install

1. Download `Jellyfin.Plugin.AniFin.dll` from the [latest release](https://github.com/RikoxCode/AniFin/releases/latest)
2. Open Jellyfin and go to `Dashboard > Plugins`
3. Click `Upload Plugin` and select the downloaded DLL
4. Restart Jellyfin
5. Configure the plugin under `Dashboard > Plugins > AniFin`

### Manual Install

1. Download the DLL from releases
2. Copy to your Jellyfin plugins directory:
   ```bash
   cp Jellyfin.Plugin.AniFin.dll /var/lib/jellyfin/plugins/
   ```
3. Restart Jellyfin
4. Configure via the web interface

## ⚙️ Requirements

- **Jellyfin**: Version 10.8.0 or higher
- **AniWorld CLI**: Must be installed at `/root/.local/bin/aniworld`
- **Permissions**: Write access to the download directory

### Installing AniWorld CLI

```bash
# Install AniWorld CLI (follow official documentation)
# Ensure it's accessible at /root/.local/bin/aniworld
```

## 🚀 Usage

### Via Web Interface

1. Open Jellyfin Dashboard
2. Navigate to `Plugins > AniFin`
3. Go to the "Download" tab
4. Enter an AniWorld URL
5. Select language and provider (optional)
6. Click "Add to Queue"

### Via Browser Extension

1. Download the extension from the plugin's "Browser Extension" tab
2. Install in Chrome/Edge/Brave
3. Navigate to an anime on AniWorld
4. Click the AniFin extension icon
5. Click "Add to Queue"

### Supported URL Types

- **Series**: `https://aniworld.to/anime/stream/one-piece`
- **Season**: `https://aniworld.to/anime/stream/one-piece/staffel-1`
- **Episode**: `https://aniworld.to/anime/stream/one-piece/staffel-1/episode-1`

## 📊 Queue Management

The plugin includes a powerful queue system:

- **Real-time Progress**: Track download progress in real-time
- **Statistics**: View total, pending, completed, and failed downloads
- **History**: Full download history with status
- **Controls**: Pause/resume queue, cancel individual downloads, retry failed items

## 🔧 Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| Download Path | Directory for downloaded anime | `/media/anime` |
| Preferred Language | Default language for downloads | German Dub |
| Preferred Provider | Default video provider | VOE |
| Max Concurrent Downloads | Maximum parallel downloads | 2 |

## 🐛 Troubleshooting

### Downloads Not Starting

1. Check that AniWorld CLI is installed correctly
2. Verify write permissions for download directory
3. Check Jellyfin and AniFin logs

### Files Not Organizing

1. Ensure sufficient disk space
2. Check file permissions
3. Review logs for errors

### Logs Location

- Jellyfin logs: `/var/log/jellyfin/`
- AniFin logs: `<plugin-directory>/logs/anifin-*.log`

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) first.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built for the Jellyfin community
- Uses AniWorld CLI for downloading
- Inspired by the need for better anime management

## 📧 Support

- 🐛 [Report a Bug](https://github.com/RikoxCode/AniFin/issues/new?template=bug_report.md)
- 💡 [Request a Feature](https://github.com/RikoxCode/AniFin/issues/new?template=feature_request.md)
- ❓ [Ask a Question](https://github.com/RikoxCode/AniFin/issues/new?labels=question)

## ⭐ Star History

If you find this project useful, please consider giving it a star!

---

Made with ❤️ for the Jellyfin community
