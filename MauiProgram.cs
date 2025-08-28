// Localização: MauiProgram.cs

using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TVPlayerMAUI.Services; // Adicionado para o M3UParser
using TVPlayerMAUI.ViewModels;
using TVPlayerMAUI.Views;

namespace TVPlayerMAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement() // Necessário para o player de vídeo
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // Adiciona logging para ajudar na depuração
        builder.Logging.AddDebug();
#endif

        // Registrando nossos serviços, ViewModel e View
        builder.Services.AddSingleton<M3UParser>(); // Novo serviço do parser
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}