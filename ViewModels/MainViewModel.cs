using CommunityToolkit.Mvvm.ComponentModel;
using DnDClient.Models;
using DnDClient.Services;

namespace DnDClient.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    string welcome;

    public MainViewModel()
    {
        string user = Preferences.Get("current_user", "default");
        Welcome = "Добро пожаловать, " + user;
    }
}