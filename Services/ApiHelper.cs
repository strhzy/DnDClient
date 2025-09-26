#if ANDROID
using Environment = Android.OS.Environment;
#elif WINDOWS
using Windows.Storage;
#endif
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DnDClient.Services;

public static class ApiHelper
{
    //private static readonly string _url = "http://192.168.1.167:5000/api";
    private static readonly string _url = "http://localhost:5000/api";

    public static T? Get<T>(string model, Guid id = default)
    {
        try
        {
            using var client = new HttpClient();
            var path = id == Guid.Empty ? $"{model}" : $"{model}/{id}";
            var resp = client.GetAsync($"{_url}/{path}").Result;

            if (resp.StatusCode != HttpStatusCode.OK) return default;
            var json = resp.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    public static bool Put<T>(string json, string model, Guid id)
    {
        try
        {
            using var client = new HttpClient();
            HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = client.PutAsync($"{_url}/{model}/{id}", body).Result;
            return resp.StatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static bool Post<T>(string json, string model)
    {
        try
        {
            using var client = new HttpClient();
            HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = client.PostAsync($"{_url}/{model}", body).Result;
            return resp.StatusCode == HttpStatusCode.Created;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static T? PostWithResponse<T>(string json, string model)
    {
        try
        {
            using var client = new HttpClient();
            HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = client.PostAsync($"{_url}/{model}", body).Result;

            if (resp.StatusCode != HttpStatusCode.Created &&
                resp.StatusCode != HttpStatusCode.OK)
                return default;

            var content = resp.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(content);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    public static bool Delete<T>(string model, Guid id)
    {
        try
        {
            using var client = new HttpClient();
            var resp = client.DeleteAsync($"{_url}/{model}/{id}").Result;
            return resp.StatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static async Task<string?> DownloadFileAsync(string model, string fileName, Guid id = default)
    {
        try
        {
            string? filePath = null;

            await Task.Run(async () =>
            {
                using var client = new HttpClient();
                var path = id == Guid.Empty ? $"{model}" : $"{model}/{id}/pdf";
                var response = await client.GetAsync($"{_url}/{path}", HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    await ShowAlert("Ошибка", "Не удалось скачать файл!");
                    return;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();

#if ANDROID
                var status = await MainThread.InvokeOnMainThreadAsync(() =>
                    Permissions.RequestAsync<Permissions.StorageWrite>()
                );

                if (status != PermissionStatus.Granted)
                {
                    await ShowAlert("Ошибка", "Нет доступа к хранилищу!");
                    return;
                }

                filePath = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath,
                    Environment.DirectoryDownloads, fileName);
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);
#elif WINDOWS
                var folder = KnownFolders.DocumentsLibrary;
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                filePath = file.Path;
                await using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    await stream.CopyToAsync(fileStream);
                }
#else
                // iOS и другие платформы
                filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);
#endif
            });

            if (filePath != null)
                await ShowAlert("Успех", $"Файл сохранён: {filePath}");

            return filePath;
        }
        catch (Exception ex)
        {
            await ShowAlert("Ошибка", $"Ошибка: {ex.Message}");
            return null; // при ошибке возвращаем null
        }
    }

    private static async Task ShowAlert(string title, string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        });
    }
}