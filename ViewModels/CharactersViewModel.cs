using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using DnDClient.Views;

namespace DnDClient.ViewModels;

public partial class CharactersViewModel : ObservableObject
{
    private readonly INavigation _navigation;
    
    [ObservableProperty]
    private ObservableCollection<PlayerCharacter> characters;
    
    [ObservableProperty]
    private PlayerCharacter selectedCharacter;
    
    [RelayCommand]
    private async Task TapCard(PlayerCharacter character)
    {
        if (character != null)
        {
            await _navigation.PushAsync(new CharacterDetailsPage(character));
        }
    }

    public CharactersViewModel(INavigation navigation)
    {
        _navigation = navigation;
        string userid = Preferences.Get("current_user_id", "default");
        Characters = ApiHelper.Get<ObservableCollection<PlayerCharacter>>("PlayerCharacter?query=");
    }
}