using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDClient.Models;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json.Nodes; // Для JsonNode, если нужно парсить динамически

namespace DnDClient.ViewModels;

public partial class CombatViewModel : ObservableObject
{
    [ObservableProperty] private bool masterMode;
    [ObservableProperty] private Combat combat;
    [ObservableProperty] private ObservableCollection<CombatLog> combatLogs = new();

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string selectedActionType;
    [ObservableProperty] private CombatParticipant selectedTarget;
    [ObservableProperty] private Attack selectedAttack;
    [ObservableProperty] private bool isPlayerTurn;
    [ObservableProperty] private int attackDamage;

    public async Task ConnectWebSocketAsync()
    {
        if (Combat == null) return; // Защита
        string uri = $"ws://localhost:5228/ws?combatId={Combat.Id}";
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        await _webSocket.ConnectAsync(new Uri(uri), _cts.Token);
        _ = ReceiveLoopAsync();
    }

    public async Task SendMessageAsync(object message)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts!.Token);
    }

    [RelayCommand]
    public async Task SendPlayerActionAsync()
    {
        if (!IsPlayerTurn || Combat == null || SelectedTarget == null || string.IsNullOrEmpty(SelectedActionType))
            return;

        var log = new CombatLog
        {
            Type = SelectedActionType,
            SourceId = Combat.CurrentParticipant.Id,
            TargetId = SelectedTarget.Id,
            Damage = AttackDamage, // Добавили установку Damage
            Message = $"Игрок {Combat.CurrentParticipant.Name} использует {SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget.Name}"
        };
        if (SelectedActionType == "attack")
            log.Message += $" (урон: {AttackDamage})";
        else if (SelectedActionType == "heal")
            log.Message += $" (лечение: {AttackDamage})";

        await SendMessageAsync(new { action = "player_move", log });
    }

    [RelayCommand]
    public async Task ConfirmActionAsync(CombatLog log)
    {
        if (!MasterMode || Combat == null) return; // Только мастер может подтверждать

        var source = Combat.Participants.FirstOrDefault(p => p.Id == log.SourceId);
        var target = Combat.Participants.FirstOrDefault(p => p.Id == log.TargetId);
        if (source == null || target == null) return;

        int damage = log.Damage ?? 0;
        if (log.Type == "attack")
            target.CurrentHitPoints -= damage;
        else if (log.Type == "heal")
            target.CurrentHitPoints += damage;

        CombatLogs.Add(log);
        await SendMessageAsync(new { action = "master_confirm", combat = Combat, log });
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[4096];
        while (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts!.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", _cts.Token);
                break;
            }
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var message = JsonDocument.Parse(json).RootElement;

            if (message.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString();
                if (action == "player_move" && MasterMode)
                {
                    // Мастер получает предложение действия — вызови UI для подтверждения
                    var logJson = message.GetProperty("log");
                    var log = JsonSerializer.Deserialize<CombatLog>(logJson.GetRawText());
                }
                else if (action == "master_confirm")
                {
                    // Все (включая мастера, но мастер уже обновил локально) обновляют состояние
                    var combatJson = message.GetProperty("combat");
                    Combat = JsonSerializer.Deserialize<Combat>(combatJson.GetRawText());
                    var logJson = message.GetProperty("log");
                    var log = JsonSerializer.Deserialize<CombatLog>(logJson.GetRawText());
                    if (!CombatLogs.Contains(log)) // Избежать дубликатов
                        CombatLogs.Add(log);
                }
            }
        }
    }

    public async Task DisconnectWebSocketAsync()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", _cts!.Token);
        }
        _cts?.Cancel();
        _cts?.Dispose();
        _webSocket?.Dispose();
    }
}