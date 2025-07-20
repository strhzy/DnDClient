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

    [ObservableProperty] private ObservableCollection<PlayerCharacter> characters;

    [RelayCommand]
    private async Task TapCard(PlayerCharacter character)
    {
        if (character != null)
        {
            await _navigation.PushAsync(new CharacterDetailsPage(character));
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var character = new PlayerCharacter();
        var stringUserId = Preferences.Get("current_user_id", string.Empty);
        if (Guid.TryParse(stringUserId, out var userId))
        {
            character.UserId = userId;
        }

        if (ApiHelper.Post<PlayerCharacter>(Serdeser.Serialize(character), "PlayerCharacter"))
        {
            characters.Add(character);
            await _navigation.PushAsync(new CharacterDetailsPage(character));
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Персонаж не добавлен", "Закрыть");
        }
    }

    [RelayCommand]
    private async Task Delete(PlayerCharacter character)
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Подтверждение",
            $"Вы уверены, что хотите удалить персонажа {character.Name}?",
            "Да",
            "Нет");
        
        if (confirm)
        {
            if (ApiHelper.Delete<PlayerCharacter>("PlayerCharacter", character.Id))
            {
                characters.Remove(character);
                await Application.Current.MainPage.DisplayAlert(
                    "Инфо",
                    "Персонаж успешно удален",
                    "Закрыть");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Не удалось удалить персонажа",
                    "Закрыть");
            }
        }
    }

    [RelayCommand]
    public async Task Export(PlayerCharacter character)
    {
        var filePath = await ApiHelper.DownloadFileAsync("PlayerCharacter", $"{character.Name}.pdf", character.Id);
        if (filePath != null)
        {
            // Файл успешно сохранён, можно что-то с ним сделать, например, открыть
            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(filePath) });
        }
    }

    public CharactersViewModel(INavigation navigation)
    {
        _navigation = navigation;
        string userid = Preferences.Get("current_user_id", "default");
        Characters = ApiHelper.Get<ObservableCollection<PlayerCharacter>>("PlayerCharacter?query=");
    }
}