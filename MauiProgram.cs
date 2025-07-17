using DnDClient.ViewModels;
using DnDClient.Views;
using Microsoft.Extensions.Logging;

namespace DnDClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Logging.AddConsole();
        builder.Services.AddTransient<CharactersPage>();
        builder.Services.AddTransient<CharactersViewModel>();
        builder.Services.AddTransient<CharacterDetailsPage>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}