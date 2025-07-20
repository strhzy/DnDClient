using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            bool success = ApiHelper.Put<PlayerCharacter>(Serdeser.Serialize(_char), "PlayerCharacter", _char.Id);
            foreach (var attack in _char.Attacks)
            {
                ApiHelper.Put<Attack>(Serdeser.Serialize(attack), "Attack", attack.Id);
            }
        }
    }

    [RelayCommand]
    async Task AddAttack()
    {
        var attack = new Attack();
        attack.PlayerCharacterId = _char.Id;
        _char.Attacks.Add(attack);
        ApiHelper.Post<Attack>(Serdeser.Serialize(attack), "Attack");
    }

    [RelayCommand]
    async Task Export()
    {
        
    }

    public CharDetailsViewModel(INavigation navigation, PlayerCharacter character)
    {
        _navigation = navigation;
        _char = character;
    }
}