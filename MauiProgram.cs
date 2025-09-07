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
        builder.Services.AddTransient<CharacterDetailsPage>();
        
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<CampaignListViewModel>();
        builder.Services.AddTransient<CampaignViewModel>();
        builder.Services.AddTransient<CharactersViewModel>();
        builder.Services.AddTransient<CharDetailsViewModel>();
        builder.Services.AddTransient<CombatViewModel>();
        builder.Services.AddTransient<CombatParticipantsViewModel>();
        builder.Services.AddTransient<EntityManagementViewModel>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}