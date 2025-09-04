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
            // Versão atual robusta (Windows sem MSIX usa AssemblyVersion)
            Version currentVersion;
            var asmVer = typeof(App).Assembly.GetName().Version;
            if (asmVer is not null)
            {
                currentVersion = asmVer;
            }
            else if (!Version.TryParse(AppInfo.Current.VersionString, out currentVersion))
            {
                currentVersion = new Version(0, 0, 0);
            }

            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var latestRelease = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (latestRelease is null || string.IsNullOrWhiteSpace(latestRelease.TagName))
                return (false, string.Empty, string.Empty);

            // Aceita 'v1.2', 'V1.2.0' etc.
            var tag = latestRelease.TagName.Trim().TrimStart('v', 'V');
            if (!Version.TryParse(tag, out var latestVersion))
                return (false, string.Empty, string.Empty);

            if (latestVersion > currentVersion)
            {
                // Ajuste a extensão conforme o que você publica: ".exe", ".msi" ou ".msixbundle"
                var installerAsset = latestRelease.Assets
                    .FirstOrDefault(a =>
                        a.DownloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        a.DownloadUrl.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) ||
                        a.DownloadUrl.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase));

                if (installerAsset is not null)
                    return (true, latestRelease.TagName, installerAsset.DownloadUrl);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao checar atualização: {ex.Message}");
        }

        return (false, string.Empty, string.Empty);
    }

}