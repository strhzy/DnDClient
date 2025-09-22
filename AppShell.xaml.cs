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
        GoToAsync("//MainPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            SecureStorage.Remove("auth_token");
            ClearGlobalData();
            ClearNavigationStack();
            ForceGarbageCollection();
            RestartApplication();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при выходе: {ex.Message}");
        }
    }

    private void ClearGlobalData()
    {
        Preferences.Clear();
        SecureStorage.RemoveAll();
    }

    private void ClearNavigationStack()
    {
        foreach (var page in Navigation.NavigationStack.ToList())
        {
            Navigation.RemovePage(page);
        }
    }


    private void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void RestartApplication()
    {
        Application.Current.MainPage = new AppShell();
    }
}