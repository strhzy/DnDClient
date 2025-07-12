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
        Welcome = "Добро пожаловать, " + SecureStorage.GetAsync("current_user").Result;
    }
}