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
}