using System.Diagnostics;
using System.Text.RegularExpressions;
using TVPlayerMAUI.Models;

namespace TVPlayerMAUI.Services
{
    public class M3UParser
    {
        private readonly HttpClient _httpClient;

        public M3UParser()
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = true };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        // O método agora aceita um objeto para reportar o progresso
        public async Task<List<Channel>> Parse(string url, IProgress<string> progress)
        {
            var channels = new List<Channel>();

            try
            {
                progress.Report("Baixando a lista de canais...");

                // OTIMIZAÇÃO: Usamos GetStreamAsync para ler a resposta da rede como um fluxo (stream)
                using var stream = await _httpClient.GetStreamAsync(url);
                using var reader = new StreamReader(stream);

                progress.Report("Analisando os canais...");
                string? line;
                Channel? currentChannel = null;
                int channelCount = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("#EXTINF"))
                    {
                        currentChannel = new Channel();
                        var infoString = line.Substring(line.IndexOf(':') + 1).Trim();
                        var lastCommaIndex = infoString.LastIndexOf(',');
                        if (lastCommaIndex != -1)
                        {
                            currentChannel.Name = infoString.Substring(lastCommaIndex + 1).Trim();
                            var attributesString = infoString.Substring(0, lastCommaIndex);
                            var attributeMatches = Regex.Matches(attributesString, "([\\w-]+)=\"(.*?)\"");
                            foreach (Match match in attributeMatches)
                            {
                                var key = match.Groups[1].Value;
                                var value = match.Groups[2].Value;
                                switch (key)
                                {
                                    case "tvg-name": currentChannel.Name = value; break;
                                    case "tvg-logo": currentChannel.Logo = value; break;
                                    case "group-title": currentChannel.Group = value; break;
                                }
                            }
                        }
                    }
                    else if (!line.StartsWith("#") && currentChannel != null && Uri.IsWellFormedUriString(line, UriKind.Absolute))
                    {
                        currentChannel.Url = line;
                        channels.Add(currentChannel);
                        currentChannel = null;
                        channelCount++;

                        // FEEDBACK DE PROGRESSO: A cada 1000 canais, envia uma atualização
                        if (channelCount % 1000 == 0)
                        {
                            progress.Report($"Analisando... {channelCount} canais encontrados.");
                        }
                    }
                }
                progress.Report($"Análise concluída. {channelCount} canais encontrados.");
            }
            catch (Exception ex)
            {
                progress.Report($"Erro: {ex.Message}");
                Debug.WriteLine($"Erro no parser: {ex}");
                throw; // Re-lança a exceção para ser tratada no ViewModel
            }

            return channels;
        }
    }
}