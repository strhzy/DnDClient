using DnDClient.Views;

namespace DnDClient;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        CheckAuthStateAsync();
    }

    private async void CheckAuthStateAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");

        if (!string.IsNullOrEmpty(token))
        {
            SetupAuthenticatedShell();
        }
    }

    public void SetupAuthenticatedShell()
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;
        Task.Delay(1000);
        GoToAsync("//MainPage");
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        SecureStorage.Remove("auth_token");
        FlyoutBehavior = FlyoutBehavior.Disabled;
        //Application.Current.MainPage = new AppShell(); // сбрасываем Shell
        GoToAsync("//AuthPage");
    }
}

