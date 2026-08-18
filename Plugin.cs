using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.UnifiedLibrary.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.UnifiedLibrary;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "Unified Library";

    public override Guid Id => Guid.Parse("AB354BE7-B44D-4C55-A548-E820558F19DA");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = "unifiedlibrary-config",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            },
            new PluginPageInfo
            {
                Name = "unifiedlibrary",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Web.unifiedPage.html", GetType().Namespace),
                DisplayName = "Все фильмы",
                EnableInMainMenu = true,
                MenuIcon = "movie"
            }
        ];
    }
}