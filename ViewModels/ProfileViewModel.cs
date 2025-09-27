using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;

namespace DnDClient.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string checkNewPassword;
    [ObservableProperty] private User currentUser;
    [ObservableProperty] private string newPassword;
    [ObservableProperty] private string newUsername;
    [ObservableProperty] private string userId;

    public ProfileViewModel()
    {
        userId = Preferences.Get("current_user_id", string.Empty);
        if (userId != string.Empty)
        {
            currentUser = ApiHelper.Get<User>("User", new Guid(userId));
        }
    }

    [RelayCommand]
    public void ChangeUsername()
    {
        if (newUsername != String.Empty)
        {
            try
            {
                currentUser.Username = newUsername;
                if (ApiHelper.Put<User>(Serdeser.Serialize(currentUser), "User", new Guid(userId)))
                {
                    Shell.Current.DisplayAlert("Успех", "Вы успешно сменили имя пользователя", "OK");
                }
                else
                {
                    Shell.Current.DisplayAlert("Ошибка", "Не удалось поменять имя пользователя", "OK");
                }
            }
            catch (Exception e)
            {
                Shell.Current.DisplayAlert("Ошибка", $"{e}", "OK");
            }
        }
        else
        {
            Shell.Current.DisplayAlert("Ошибка", "Вы не ввели новое имя пользователя", "OK");
        }
    }

    [RelayCommand]
    public void ChangePassword()
    {
        if (newPassword == checkNewPassword && newPassword != String.Empty && checkNewPassword != String.Empty)
        {
            try
            {
                currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                if (ApiHelper.Put<User>(Serdeser.Serialize(currentUser), "User", new Guid(userId)))
                {
                    Shell.Current.DisplayAlert("Успех", "Вы успешно сменили пароль", "OK");
                }
                else
                {
                    Shell.Current.DisplayAlert("Ошибка", "Не удалось поменять пароль", "OK");
                }
            }
            catch (Exception e)
            {
                Shell.Current.DisplayAlert("Ошибка", $"{e}", "OK");
            }
        }
        else
        {
            Shell.Current.DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
        }
    }
}