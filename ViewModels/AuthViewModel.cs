using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using Newtonsoft.Json;
using DnDClient.Services;

namespace DnDClient.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    [RelayCommand]
    async Task Login()
    {
        try
        {
            var loginData = new { Email, PasswordHash = Password };
            var json = JsonConvert.SerializeObject(loginData);
            var response = ApiHelper.PostWithResponse<LoginResponse>(json, "Auth/login");
            
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                await SecureStorage.SetAsync("auth_token", response.Token);
                await SecureStorage.SetAsync("current_user", Email);
                await SecureStorage.SetAsync("current_user_id", ApiHelper.Get<List<User>>("User?email="+Email).FirstOrDefault().Id.ToString());
                var shell = (AppShell)Shell.Current;
                shell.FlyoutBehavior = FlyoutBehavior.Flyout;
                await shell.GoToAsync("//MainPage");
            }
            else
            {
                Message = "Авторизация провалена! Проверьте данные или попробуйте позже";
            }
        }
        catch (Exception ex)
        {
            Message = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    async Task Register()
    {
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            var registerData = new { Email, PasswordHash = passwordHash };
            var json = JsonConvert.SerializeObject(registerData);
            var success = ApiHelper.Post<object>(json, "Auth/register");

            if (success)
            {
                Message = "Регистрация успешна! Пожалуйста, войдите в свой аккаунт";
            }
            else
            {
                Message = "Регистрация провалена!";
            }
        }
        catch (Exception ex)
        {
            Message = $"Ошибка: {ex.Message}";
        }
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}