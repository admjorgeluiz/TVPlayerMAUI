using System.Text.Json.Serialization;

namespace TVPlayerMAUI.Models;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; } = new();
}

public class Asset
{
    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}