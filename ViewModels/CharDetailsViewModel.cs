using CommunityToolkit.Mvvm.ComponentModel;
using DnDClient.Models;
using DnDClient.Services;

namespace DnDClient.ViewModels;

public partial class CharDetailsViewModel : ObservableObject
{
    private readonly INavigation _navigation;
    
    [ObservableProperty]
    private PlayerCharacter _char;
    
    private PlayerCharacter oldChar;

    [ObservableProperty] 
    private bool editMode = false;

    partial void OnEditModeChanged(bool value)
    {
        if (value)
        {
            oldChar = new PlayerCharacter();
            oldChar = _char;
        }
        else if (!value)
        {
            ApiHelper.Put<PlayerCharacter>(Serdeser.Serialize(_char), "PlayerCharacter", _char.Id);
        }
    }

    public CharDetailsViewModel(INavigation navigation, PlayerCharacter character)
    {
        _navigation = navigation;
        _char = character;
    }
}