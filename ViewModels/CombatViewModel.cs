using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using DnDClient.Views;

namespace DnDClient.ViewModels;

public partial class CombatViewModel : ObservableObject, IAsyncDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _listenTask;

    [ObservableProperty] private int attackDamage;
    [ObservableProperty] private Combat combat;
    [ObservableProperty] private ObservableCollection<CombatLog> combatLogs = new();
    [ObservableProperty] private bool isPlayerTurn;
    [ObservableProperty] private bool masterMode;
    [ObservableProperty] private ObservableCollection<CombatLog> pendingLogs = new();
    [ObservableProperty] private string selectedActionType;
    [ObservableProperty] private Attack selectedAttack;
    [ObservableProperty] private CombatParticipant selectedEnemy;
    [ObservableProperty] private CombatParticipant selectedNpc;
    [ObservableProperty] private CombatParticipant selectedTarget;

    public CombatViewModel(Combat _combat, bool _masterMode = false)
    {
        if (_combat != null)
        {
            Combat = _combat;
            MasterMode = _masterMode;
            IsPlayerTurn = !MasterMode && Combat.CurrentParticipant.Type == ParticipantType.Player;
            CombatLogs = Combat.CombatLogs ?? new();
            combat.CombatLogs = CombatLogs;
        }

        ConnectAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    public async Task ConnectAsync()
    {
        if (Combat == null) return;

        _cts = new CancellationTokenSource();
        int retryCount = 0;
        const int maxRetries = int.MaxValue; // Retry indefinitely, adjust if needed
        const int retryDelaySeconds = 5;

        while (!_cts.Token.IsCancellationRequested && retryCount < maxRetries)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout for HTTP request

            Console.WriteLine($"Подключаюсь к SSE (Попытка {retryCount + 1})...");

            try
            {
                using var response = await client.GetAsync(
                    $"http://strhzy.ru:5100/api/Combat/{Combat.Id}/stream",
                    HttpCompletionOption.ResponseHeadersRead,
                    _cts.Token);

                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
                var reader = new StreamReader(stream, System.Text.Encoding.UTF8);

                Console.WriteLine("Поток подключен, слушаю события...");

                _listenTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_cts.Token.IsCancellationRequested)
                        {
                            var readTask = reader.ReadLineAsync();
                            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15), _cts.Token); // Increased timeout for SSE keep-alive

                            var completedTask = await Task.WhenAny(readTask, timeoutTask);
                            if (completedTask == timeoutTask)
                            {
                                Console.WriteLine("Таймаут чтения строки, проверка соединения...");
                                break; // Trigger reconnection on timeout
                            }

                            var line = await readTask;
                            if (line == null)
                            {
                                Console.WriteLine("Соединение закрыто сервером, попытка переподключения...");
                                break; // Trigger reconnection
                            }

                            if (string.IsNullOrWhiteSpace(line))
                            {
                                Console.WriteLine("Получена пустая строка, возможно keep-alive...");
                                continue; // Handle SSE delimiter or keep-alive
                            }

                            Console.WriteLine($"[RAW LINE]: '{line}' (Length: {line.Length})");

                            if (line.StartsWith("data:"))
                            {
                                var json = line.Substring(5).Trim();
                                if (!string.IsNullOrEmpty(json))
                                {
                                    try
                                    {
                                        Console.WriteLine($"[PARSED JSON]: {json}");
                                        using var doc = JsonDocument.Parse(json);
                                        if (doc.RootElement.TryGetProperty("log", out var logElement))
                                        {
                                            var logJson = logElement.GetRawText();
                                            var log = JsonSerializer.Deserialize<CombatLog>(logJson);
                                            if (log != null)
                                            {
                                                MainThread.BeginInvokeOnMainThread(() =>
                                                {
                                                    CombatLogs.Add(log);
                                                });
                                            }
                                            else
                                            {
                                                Console.WriteLine("Ошибка: CombatLog не удалось десериализовать");
                                            }
                                        }
                                        if (doc.RootElement.TryGetProperty("combat", out var combatElement))
                                        {
                                            var combatJson = combatElement.GetRawText();
                                            var newCombat = JsonSerializer.Deserialize<Combat>(combatJson);
                                            if (newCombat != null)
                                            {
                                                MainThread.BeginInvokeOnMainThread(() =>
                                                {
                                                    Combat.Participants = newCombat.Participants;
                                                });
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Поле 'log' не найдено в JSON");
                                        }
                                    }
                                    catch (JsonException ex)
                                    {
                                        Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}, JSON: {json}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Общая ошибка при обработке JSON: {ex.Message}, JSON: {json}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Получен пустой JSON после 'data:'");
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Операция отменена");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при чтении потока: {ex.Message}");
                        ConnectAsync();
                    }
                }, _cts.Token);

                await _listenTask;
                retryCount++;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка HTTP подключения: {ex.Message}");
                retryCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                retryCount++;
            }

            client.Dispose();

            if (!_cts.Token.IsCancellationRequested && retryCount < maxRetries)
            {
                Console.WriteLine($"Переподключение через {retryDelaySeconds} секунд...");
                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), _cts.Token);
            }
        }

        if (retryCount >= maxRetries)
        {
            Console.WriteLine($"Достигнуто максимальное количество попыток переподключения ({maxRetries})");
        }
    }



    [RelayCommand]
    public async Task SendPlayerActionAsync()
    {
        try
        {
            if (!IsPlayerTurn || Combat == null || SelectedTarget == null)
                return;
            if (AttackDamage > 0) SelectedActionType = "attack";
            else if (AttackDamage < 0) SelectedActionType = "heal";
            var log = new CombatLog
            {
                CombatId = Combat.Id,
                Type = SelectedActionType,
                SourceId = Combat.CurrentParticipant.Id,
                TargetId = SelectedTarget.Id,
                Damage = AttackDamage,
                Message = $"Игрок {Combat.CurrentParticipant.Name} использует " +
                          $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget.Name}" +
                          $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
            };

            ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), $"Combat/{Combat.Id}/player-move");
        }
        catch (Exception e)
        {
            
        }
    }

    [RelayCommand]
    public async Task ConfirmActionAsync(CombatLog log)
    {
        if (!MasterMode || Combat == null) return;

        var request = new
        {
            CombatId = Combat.Id,
            Combat,
            Log = log
        };

        ApiHelper.Post<object>(Serdeser.Serialize(request), "combat/master-confirm");
    }

    [RelayCommand]
    public async Task SendNpcActionAsync()
    {
        try
        {
            if (!MasterMode || Combat == null || SelectedNpc == null)
                return;
            if (AttackDamage > 0) SelectedActionType = "attack";
            else if (AttackDamage < 0) { 
                SelectedActionType = "heal";
                AttackDamage *= -1;
            }
            var log = new CombatLog
            {
                Id = Guid.Empty,
                CombatId = Combat.Id,
                Type = SelectedActionType,
                SourceId = SelectedNpc.Id,
                TargetId = SelectedTarget.Id,
                Damage = AttackDamage,
                Message = $"НПС {SelectedNpc.Name} использует " +
                          $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget?.Name ?? "цель"}" +
                          $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
            };
            string json = Serdeser.Serialize(log);
            bool success = ApiHelper.Post<CombatLog>(json, $"Combat/{Combat.Id}/npc-move");
        }
        catch(Exception e){}
    }

    [RelayCommand]
    public async Task SendEnemyActionAsync()
    {
        try
        {
            if (!MasterMode || Combat == null || SelectedEnemy == null)
                return;
            if (AttackDamage > 0) SelectedActionType = "attack";
            else if (AttackDamage < 0) SelectedActionType = "heal";
            var log = new CombatLog
            {
                CombatId = Combat.Id,
                Type = SelectedActionType,
                SourceId = SelectedEnemy.Id,
                TargetId = SelectedTarget.Id,
                Damage = AttackDamage,
                Message = $"Враг {SelectedEnemy.Name} использует " +
                          $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget?.Name ?? "цель"}" +
                          $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
            };

            ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), $"Combat/{Combat.Id}/enemy-move");
        }
        catch(Exception e){}
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();
        if (_listenTask != null) await _listenTask;
        _cts?.Dispose();
    }
    
    [RelayCommand]
    public async Task ManageParticipants()
    {
        if (combat != null && Shell.Current.Navigation.NavigationStack.LastOrDefault() is not CombatParticipantsPage)
        {
            await Shell.Current.Navigation.PushAsync(new CombatParticipantsPage(combat));
        }
    }
}

public class CombatEvent
{
    public string EventType { get; set; }
    public Combat Combat { get; set; }
    public CombatLog Log { get; set; }
}