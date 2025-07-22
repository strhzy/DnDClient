using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DnDClient.Services;

public static class ApiHelper
{
    //private static readonly string _url = "http://192.168.1.167:5000/api";
    private static readonly string _url = "http://localhost:5228/api";

    public static T? Get<T>(string model, Guid id = default)
    {
        var client = new HttpClient();
        var path = id == Guid.Empty ? $"{model}" : $"{model}/{id}";
        var response = client.GetAsync($"{_url}/{path}").Result;
        if (response.StatusCode != HttpStatusCode.OK) return default;
        return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
    }

    public static bool Put<T>(string json, string model, Guid id)
    {
        var client = new HttpClient();
        HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = client.PutAsync($"{_url}/{model}/{id}", body).Result;
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public static bool Post<T>(string json, string model)
    {
        var client = new HttpClient();
        HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = client.PostAsync($"{_url}/{model}", body).Result;
        return response.StatusCode == HttpStatusCode.Created;
    }

    public static T? PostWithResponse<T>(string json, string model)
    {
        var client = new HttpClient();
        HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = client.PostAsync($"{_url}/{model}", body).Result;
        if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            return default;
        return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
    }

    public static bool Delete<T>(string model, Guid id)
    {
        var client = new HttpClient();
        var response = client.DeleteAsync($"{_url}/{model}/{id}").Result;
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public static async Task<string?> DownloadFileAsync(string model, string fileName, Guid id = default)
    {
        try
        {
            string filePath = null;

            await Task.Run(async () =>
            {
                using var client = new HttpClient();
                var path = id == Guid.Empty ? $"{model}" : $"{model}/{id}/pdf";
                var response = await client.GetAsync($"{_url}/{path}", HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    await ShowAlert("Ошибка", "Не удалось скачать файл!");
                    return; // Исправлено: просто return без значения
                }

                await using var stream = await response.Content.ReadAsStreamAsync();

#if ANDROID
                var status = await MainThread.InvokeOnMainThreadAsync(() =>
                    Permissions.RequestAsync<Permissions.StorageWrite>()
                );

                if (status != PermissionStatus.Granted)
                {
                    await ShowAlert("Ошибка", "Нет доступа к хранилищу!");
                    return; // Исправлено: просто return без значения
                }

                filePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                    Android.OS.Environment.DirectoryDownloads, fileName);
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);
#elif WINDOWS
            var folder = Windows.Storage.KnownFolders.DocumentsLibrary;
            var file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            filePath = file.Path;
            await using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                await stream.CopyToAsync(fileStream);
            }
#else
            // Для iOS и других платформ
            filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }
#endif
            });

            if (filePath != null)
            {
                await ShowAlert("Успех", $"Файл сохранён: {filePath}");
            }

            return filePath;
        }
        catch (Exception ex)
        {
            await ShowAlert("Ошибка", $"Ошибка: {ex.Message}");
            return null;
        }
    }

    private static async Task ShowAlert(string title, string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        });
    }
}