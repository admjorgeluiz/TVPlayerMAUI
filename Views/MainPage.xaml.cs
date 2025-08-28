using System.ComponentModel;
using TVPlayerMAUI.ViewModels;
using CommunityToolkit.Maui.Core; // Adicionado para MediaStateChangedEventArgs
using CommunityToolkit.Maui.Core.Primitives;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; // Adicionado para o DeviceDisplay

#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Microsoft.UI;
#endif

namespace TVPlayerMAUI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private IDispatcherTimer? _hideControlsTimer;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        InitializeHideTimer();
        this.Loaded += MainPage_Loaded;
        this.Unloaded += MainPage_Unloaded;
    }

    private void MainPage_Loaded(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        SetupKeyboardHooks();
    }

    private void MainPage_Unloaded(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        CleanupKeyboardHooks();
        _hideControlsTimer?.Stop();
    }

    // NOVO MÉTODO PARA MANTER A TELA ACESA
    private void MediaPlayer_StateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        // Se o novo estado for "Tocando"
        if (e.NewState == MediaElementState.Playing)
        {
            // Pede para o dispositivo manter a tela acesa
            DeviceDisplay.Current.KeepScreenOn = true;
        }
        // Para qualquer outro estado (Pausado, Parado, Falha, etc.)
        else
        {
            // Libera o dispositivo para seguir suas configurações de energia normais
            DeviceDisplay.Current.KeepScreenOn = false;
        }
    }

    private void SetupKeyboardHooks()
    {
#if WINDOWS
        var window = this.GetParentWindow();
        if (window?.Handler?.PlatformView is MauiWinUIWindow nativeWindow && nativeWindow.Content is FrameworkElement root)
        {
            root.KeyDown -= Root_KeyDown;
            root.KeyDown += Root_KeyDown;
        }
#endif
    }

    private void CleanupKeyboardHooks()
    {
#if WINDOWS
        var window = this.GetParentWindow();
        if (window?.Handler?.PlatformView is MauiWinUIWindow nativeWindow && nativeWindow.Content is FrameworkElement root)
        {
            root.KeyDown -= Root_KeyDown;
        }
#endif
    }

#if WINDOWS
    private void Root_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Space:
                if (!M3uUrlEntry.IsFocused && !ChannelSearchBar.IsFocused)
                {
                    TogglePlayPause();
                    e.Handled = true;
                }
                break;
            case VirtualKey.Escape:
                if (_viewModel.IsVideoFullScreen)
                {
                    _viewModel.ToggleVideoFullScreenCommand.Execute(null);
                    e.Handled = true;
                }
                break;
            case VirtualKey.F11:
                _viewModel.ToggleVideoFullScreenCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
#endif

    private void InitializeHideTimer()
    {
        _hideControlsTimer = Dispatcher.CreateTimer();
        _hideControlsTimer.Interval = TimeSpan.FromSeconds(3);
        _hideControlsTimer.Tick += async (s, e) =>
        {
            _hideControlsTimer.Stop();
            if (FullScreenOverlay is not null && _viewModel.IsVideoFullScreen)
                await FullScreenOverlay.FadeTo(0);
        };
    }

    private void FullScreen_PointerMoved(object sender, PointerEventArgs e)
    {
        if (FullScreenOverlay is not null)
            FullScreenOverlay.Opacity = 1.0;

        _hideControlsTimer?.Start();
    }

    private void PlayPauseButton_Clicked(object sender, EventArgs e)
    {
        TogglePlayPause();
    }

    private void MuteButton_Clicked(object sender, EventArgs e)
    {
        mediaPlayer.ShouldMute = !mediaPlayer.ShouldMute;
        MuteButton.Text = mediaPlayer.ShouldMute ? "Som" : "Mudo";
    }

    private void TogglePlayPause()
    {
        if (mediaPlayer.CurrentState == MediaElementState.Playing)
        {
            mediaPlayer.Pause();
            PlayPauseButton.Text = "Play";
        }
        else if (mediaPlayer.CurrentState is MediaElementState.Paused or MediaElementState.Stopped)
        {
            mediaPlayer.Play();
            PlayPauseButton.Text = "Pause";
        }
    }

    private void timelineSlider_DragCompleted(object sender, EventArgs e)
    {
        if (sender is Slider slider)
        {
            mediaPlayer.SeekTo(TimeSpan.FromSeconds(slider.Value));
        }
    }

    private void mediaPlayer_PositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        _viewModel.Position = e.Position;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPageViewModel.IsVideoFullScreen))
        {
            if (_viewModel.IsVideoFullScreen) { EnterFullScreen(); } else { ExitFullScreen(); }
        }
    }

    public void EnterFullScreen()
    {
#if WINDOWS
        var window = this.GetParentWindow();
        if (window?.Handler?.PlatformView is MauiWinUIWindow nativeWindow)
        {
            var appWindow = GetAppWindow(nativeWindow);
            appWindow?.SetPresenter(AppWindowPresenterKind.FullScreen);
            FullScreenOverlay.Opacity = 1.0;
            _hideControlsTimer?.Start();
            mediaPlayer.Focus();
        }
#endif
    }

    public void ExitFullScreen()
    {
#if WINDOWS
        var window = this.GetParentWindow();
        if (window?.Handler?.PlatformView is MauiWinUIWindow nativeWindow)
        {
            var appWindow = GetAppWindow(nativeWindow);
            appWindow?.SetPresenter(AppWindowPresenterKind.Overlapped);
            mediaPlayer.Focus();
        }
#endif
    }

#if WINDOWS
    private AppWindow GetAppWindow(MauiWinUIWindow nativeWindow)
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }
#endif
}