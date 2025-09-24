using DnDClient.Views;

namespace DnDClient;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Routing.RegisterRoute("CreateNPCPage", typeof(CreateNPCPage));
        Routing.RegisterRoute("CreateEnemyPage", typeof(CreateEnemyPage));
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        var token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//AuthPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}