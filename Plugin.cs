using System;
using System.Collections.Generic;
using Jellyfin.Plugin.AniFin.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AniFin
{
    /// <summary>
    /// AniFin Plugin for Jellyfin - Download anime from AniWorld to Jellyfin.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            FileLogger.Initialize();
            FileLogger.Info("AniFin Plugin initialized");
        }

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public override string Name => "AniFin";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        /// <summary>
        /// Gets the plugin web pages.
        /// </summary>
        /// <returns>The plugin web pages.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "anifin",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.anifin.html"
                },
                new PluginPageInfo
                {
                    Name = "anifinjs",  // ← Anderer Name!
                    EmbeddedResourcePath = GetType().Namespace + ".Web.anifin.js"
                }
            };
        }
    }
}