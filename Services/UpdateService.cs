// Localização: Services/UpdateService.cs
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
        // A API do GitHub exige um User-Agent
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TVPlayerMAUI-Updater");
    }

    public async Task<(bool IsUpdateAvailable, string NewVersion, string DownloadUrl)> CheckForUpdate()
    {
        try
        {
            // 1. Pega a versão atual do nosso próprio aplicativo
            var currentVersion = new Version(AppInfo.Current.VersionString);

            // 2. Consulta a API do GitHub para a última release
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var latestRelease = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (latestRelease is null || string.IsNullOrWhiteSpace(latestRelease.TagName))
            {
                return (false, string.Empty, string.Empty);
            }

            // 3. Compara as versões
            // Remove o 'v' inicial da tag do GitHub (ex: "v0.2.0" -> "0.2.0")
            var latestVersion = new Version(latestRelease.TagName.Replace("v", ""));

            if (latestVersion > currentVersion)
            {
                // Encontra o link do nosso instalador .exe
                var installerAsset = latestRelease.Assets.FirstOrDefault(a => a.DownloadUrl.EndsWith(".exe"));
                if (installerAsset is not null)
                {
                    return (true, latestRelease.TagName, installerAsset.DownloadUrl);
                }
            }
        }
        catch (Exception ex)
        {
            // Lida com erros de internet ou de parsing
            System.Diagnostics.Debug.WriteLine($"Erro ao checar atualização: {ex.Message}");
        }

        return (false, string.Empty, string.Empty);
    }
}