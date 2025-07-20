using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DnDClient.Services;

public static class ApiHelper
{
    private static readonly string _url = "http://192.168.1.167:5000/api";

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
            using var client = new HttpClient();
            var path = id == Guid.Empty ? $"{model}" : $"{model}/{id}/pdf";
            var response = await client.GetAsync($"{_url}/{path}", HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось скачать файл!", "OK");
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

#if ANDROID
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Нет доступа к хранилищу!", "OK");
                return null;
            }

            filePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                Android.OS.Environment.DirectoryDownloads, fileName);
    #elif WINDOWS
            var folder = Windows.Storage.KnownFolders.DocumentsLibrary;
            var file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            filePath = file.Path;
            await using var fileStream = await file.OpenStreamForWriteAsync();
            await stream.CopyToAsync(fileStream);
            return filePath;
#else
            // iOS или другие: используем кэш
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
#endif

            await Application.Current.MainPage.DisplayAlert("Успех", $"Файл сохранён: {filePath}", "OK");
            return filePath;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", $"Что-то пошло не так: {ex.Message}", "OK");
            return null;
        }
    }
}