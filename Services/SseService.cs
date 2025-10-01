using System.Net.Http;
using System.Text;
using System.Threading;

namespace DnDClient.Services;

public static class SseClient
{
    private static HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
    })
    {
        Timeout = Timeout.InfiniteTimeSpan
    };
    
    public static async Task ConnectAsync(string url, Action<string> onMessage, CancellationToken token)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url,
                HttpCompletionOption.ResponseHeadersRead,
                token);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var buffer = new char[1];
            var sb = new StringBuilder();

            while (!token.IsCancellationRequested &&
                   await reader.ReadAsync(buffer, 0, 1) > 0)
            {
                char c = buffer[0];
                sb.Append(c);

                if (c == '\n') // получили строку
                {
                    var line = sb.ToString().Trim();
                    sb.Clear();

                    if (line.StartsWith("data:"))
                    {
                        var json = line.Substring(5).Trim();
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            try
                            {
                                onMessage?.Invoke(json);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка в обработчике SSE: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("SSE соединение остановлено (отмена)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка SSE: {ex.Message}");
        }
    }
}
