using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AniFin.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the download path.
        /// </summary>
        public string DownloadPath { get; set; } = "/media/anime";
        
        /// <summary>
        /// Gets the available languages (read-only).
        /// </summary>
        public List<string> AvailablyLanguages { get; set; } = new() 
        { 
            "German Dub", 
            "German Sub", 
            "English Sub" 
        };
        
        /// <summary>
        /// Gets or sets the preferred language.
        /// </summary>
        public string PreferredLanguage { get; set; } = "German Dub";
        
        /// <summary>
        /// Gets the available providers (read-only).
        /// </summary>
        public List<string> AvailablyProviders { get; set; } = new() 
        { 
            "VOE", 
            "Vidoza", 
            "Streamtape", 
            "Doodstream" 
        };
        
        /// <summary>
        /// Gets or sets the preferred provider.
        /// </summary>
        public string PreferredProvider { get; set; } = "VOE";
        
        /// <summary>
        /// Gets or sets the maximum concurrent downloads.
        /// </summary>
        public int MaxConcurrentDownloads { get; set; } = 2;
        
        /// <summary>
        /// Gets whether the queue should auto-process (read-only, always true).
        /// </summary>
        public bool AutoProcessQueue { get; set; } = true;
    }
}