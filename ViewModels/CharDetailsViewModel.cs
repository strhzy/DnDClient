using CommunityToolkit.Mvvm.ComponentModel;
using DnDClient.Models;

namespace DnDClient.ViewModels;

public partial class CharDetailsViewModel : ObservableObject
{
    private readonly INavigation _navigation;
    
    [ObservableProperty]
    private PlayerCharacter _char;

    public CharDetailsViewModel(INavigation navigation, PlayerCharacter character)
    {
        _navigation = navigation;
        _char = character;
    }
}