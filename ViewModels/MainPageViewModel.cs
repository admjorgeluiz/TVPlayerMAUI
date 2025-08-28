using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TVPlayerMAUI.Models;
using TVPlayerMAUI.Services;
using Microsoft.Maui.Dispatching;
using System.Threading;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

namespace TVPlayerMAUI.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly M3UParser _m3uParser;
        private readonly UpdateService _updateService;
        private List<Channel> _allChannels = new();
        private List<ChannelGroupSummary> _allGroupSummaries = new();
        private CancellationTokenSource? _searchCancellationTokenSource;

        // Propriedades para as listas
        [ObservableProperty]
        private ObservableCollection<ChannelGroupSummary> _groupSummaries = new();

        [ObservableProperty]
        private ObservableCollection<Channel> _channelsInView = new();

        // Propriedades de estado da UI
        [ObservableProperty]
        private bool _isShowingGroups = true;

        [ObservableProperty]
        private bool _isSearchResults = false;

        [ObservableProperty]
        private bool _isVideoFullScreen;

        public bool IsShowingChannelsInGroup => !IsShowingGroups && !IsSearchResults;
        public bool IsShowingChannels => !IsShowingGroups;

        [ObservableProperty]
        private string _headerTitle = "Grupos";

        // Propriedades de estado do player e carregamento
        [ObservableProperty]
        private string? _videoSource;

        [ObservableProperty]
        private string? _m3uUrl;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _loadingStatus;

        [ObservableProperty]
        private string? _searchText;

        // Propriedades para item selecionado
        [ObservableProperty]
        private Channel? _selectedChannel;

        [ObservableProperty]
        private ChannelGroupSummary? _selectedGroup;

        [ObservableProperty]
        private TimeSpan _position;

        public MainPageViewModel(M3UParser m3uParser, UpdateService updateService)
        {
            _m3uParser = m3uParser;
            _updateService = updateService;

            M3uUrl = Preferences.Get("LastM3uUrl", string.Empty);
            if (!string.IsNullOrWhiteSpace(M3uUrl))
            {
                Task.Run(LoadChannelsAsync);
            }
        }

        [RelayCommand]
        private async Task LoadChannelsAsync()
        {
            if (string.IsNullOrWhiteSpace(M3uUrl) || IsLoading) return;
            IsLoading = true;
            try
            {
                var progress = new Progress<string>(status => LoadingStatus = status);
                _allChannels = await _m3uParser.Parse(M3uUrl, progress);
                await MainThread.InvokeOnMainThreadAsync(() => LoadingStatus = "Criando lista de grupos...");
                await Task.Run(() =>
                {
                    _allGroupSummaries = _allChannels
                        .GroupBy(c => c.Group ?? "Sem Grupo")
                        .OrderBy(g => g.Key)
                        .Select(g => new ChannelGroupSummary { Name = g.Key, ChannelCount = g.Count() })
                        .ToList();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        GroupSummaries = new ObservableCollection<ChannelGroupSummary>(_allGroupSummaries);
                        GoBackToGroups();
                    });
                });
                Preferences.Set("LastM3uUrl", M3uUrl);
            }
            catch (Exception ex)
            {
                LoadingStatus = $"Falha ao carregar: {ex.Message}";
                await Task.Delay(5000);
            }
            finally
            {
                IsLoading = false;
                LoadingStatus = string.Empty;
            }
        }

        [RelayCommand]
        private void SelectGroup(ChannelGroupSummary? groupSummary)
        {
            if (groupSummary == null) return;
            var channelsInGroup = _allChannels.Where(c => (c.Group ?? "Sem Grupo") == groupSummary.Name).ToList();
            ChannelsInView = new ObservableCollection<Channel>(channelsInGroup);
            HeaderTitle = groupSummary.Name;
            IsShowingGroups = false;
            IsSearchResults = false;
            OnPropertyChanged(nameof(IsShowingChannelsInGroup));
            OnPropertyChanged(nameof(IsShowingChannels));
        }

        [RelayCommand]
        private void GoBackToGroups()
        {
            HeaderTitle = "Grupos";
            SearchText = string.Empty;
            IsShowingGroups = true;
            IsSearchResults = false;
            OnPropertyChanged(nameof(IsShowingChannelsInGroup));
            OnPropertyChanged(nameof(IsShowingChannels));
        }

        [RelayCommand]
        private void SelectChannel(Channel? channel)
        {
            if (channel == null) return;
            VideoSource = channel.Url;
        }

        [RelayCommand]
        private void ToggleVideoFullScreen()
        {
            IsVideoFullScreen = !IsVideoFullScreen;
        }
        partial void OnSearchTextChanged(string? value)
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;
            Task.Delay(300, cancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                MainThread.BeginInvokeOnMainThread(FilterChannels);
            }, cancellationToken);
        }

        private void FilterChannels()
        {
            var searchText = SearchText?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                GoBackToGroups();
                return;
            }
            var filteredChannels = _allChannels.Where(c => c.Name?.ToLowerInvariant().Contains(searchText) ?? false).ToList();
            ChannelsInView = new ObservableCollection<Channel>(filteredChannels);
            HeaderTitle = $"Resultados para: '{SearchText}'";
            IsShowingGroups = false;
            IsSearchResults = true;
            OnPropertyChanged(nameof(IsShowingChannelsInGroup));
            OnPropertyChanged(nameof(IsShowingChannels));
        }

        [RelayCommand]
        private async Task ShowSettings()
        {
            if (App.Current?.MainPage is null) return;

            var (isUpdateAvailable, newVersion, downloadUrl) = await _updateService.CheckForUpdate();

            if (isUpdateAvailable)
            {
                bool download = await App.Current.MainPage.DisplayAlert(
                    "Atualização Disponível!",
                    $"Uma nova versão ({newVersion}) foi encontrada. Deseja ir para a página de download agora?",
                    "Sim, baixar!",
                    "Agora não");

                if (download)
                {
                    await Launcher.OpenAsync(new Uri(downloadUrl));
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Tudo Certo!", "Você já está com a versão mais recente do TV Player.", "OK");
            }
        }
    }
}