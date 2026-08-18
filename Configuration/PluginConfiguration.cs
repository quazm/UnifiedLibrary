using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.UnifiedLibrary.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool Enabled { get; set; } = true;

    // ID библиотек, которые нужно объединить (заполняется через конфиг XML или будущий Web UI)
    public string[] IncludedLibraryIds { get; set; } = [];

    // Типы контента (Movie, Series, Audio и т.д.)
    public BaseItemKind[] IncludedItemTypes { get; set; } = [BaseItemKind.Movie];

    public int DefaultPageSize { get; set; } = 100;
}