namespace Jellyfin.Plugin.UnifiedLibrary.Api;

public class UnifiedItemsRequest
{
    public int StartIndex { get; set; } = 0;
    public int Limit { get; set; } = 100;
    public string SortBy { get; set; } = "SortName";
    public string SortOrder { get; set; } = "Ascending";
    public string? SearchTerm { get; set; }
    public string? Fields { get; set; }
    public string[] Genres { get; set; } = [];
    public int[] Years { get; set; } = [];
    public bool? IsFavorite { get; set; }
    public bool? IsPlayed { get; set; }      // ← ДОБАВЛЕНО
    public bool? IsUnplayed { get; set; }    // ← ДОБАВЛЕНО
}