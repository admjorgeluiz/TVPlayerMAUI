using System.Text.Json;
using TVPlayerMAUI.Models;

namespace TVPlayerMAUI.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/admjorgeluiz/TVPlayerMAUI/releases/latest";

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TVPlayerMAUI-Updater");
    }

    public async Task<(bool IsUpdateAvailable, string NewVersion, string DownloadUrl)> CheckForUpdate()
    {
        try
        {
            var currentVersion = new Version(AppInfo.Current.VersionString);

            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var latestRelease = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (latestRelease is null || string.IsNullOrWhiteSpace(latestRelease.TagName))
            {
                return (false, string.Empty, string.Empty);
            }

            var latestVersion = new Version(latestRelease.TagName.Replace("v", ""));

            if (latestVersion > currentVersion)
            {
                var installerAsset = latestRelease.Assets.FirstOrDefault(a => a.DownloadUrl.EndsWith(".exe"));
                if (installerAsset is not null)
                {
                    return (true, latestRelease.TagName, installerAsset.DownloadUrl);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao checar atualização: {ex.Message}");
        }

        return (false, string.Empty, string.Empty);
    }
}