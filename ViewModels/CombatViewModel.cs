using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;

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
            CombatLogs = ApiHelper.Get<ObservableCollection<CombatLog>>("CombatLog");
            combat.CombatLogs = CombatLogs;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    public async Task ConnectAsync()
    {
        if (Combat == null) return;

        _cts = new CancellationTokenSource();

        var client = new HttpClient();
        var stream = await client.GetStreamAsync($"http://localhost:5000/api/combat/stream/{Combat.Id}", _cts.Token);
        var reader = new StreamReader(stream);

        _listenTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data:")) continue;

                var json = line.Substring(5).Trim();
                try
                {
                    var evt = JsonSerializer.Deserialize<CombatEvent>(json);
                    if (evt == null) continue;

                    switch (evt.EventType)
                    {
                        case "PlayerMove":
                            if (MasterMode && evt.Log != null) CombatLogs.Add(evt.Log);
                            break;
                        case "MasterConfirm":
                            if (evt.Combat != null) Combat = evt.Combat;
                            if (evt.Log != null && !CombatLogs.Contains(evt.Log)) CombatLogs.Add(evt.Log);
                            break;
                        case "NpcMove":
                        case "EnemyMove":
                            if (evt.Log != null && !CombatLogs.Contains(evt.Log)) CombatLogs.Add(evt.Log);
                            if (evt.Combat != null) Combat = evt.Combat;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Parse error: {ex.Message}");
                }
            }
        }, _cts.Token);
    }

    [RelayCommand]
    public async Task SendPlayerActionAsync()
    {
        if (!IsPlayerTurn || Combat == null || SelectedTarget == null ||
            string.IsNullOrEmpty(SelectedActionType))
            return;

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

        ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "combat/player-move");
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
        if (!MasterMode || Combat == null || SelectedNpc == null ||
            string.IsNullOrEmpty(SelectedActionType))
            return;

        var log = new CombatLog
        {
            CombatId = Combat.Id,
            Type = SelectedActionType,
            SourceId = SelectedNpc.Id,
            TargetId = SelectedTarget.Id,
            Damage = AttackDamage,
            Message = $"НПС {SelectedNpc.Name} использует " +
                      $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget?.Name ?? "цель"}" +
                      $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
        };

        ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "combat/npc-move");
    }

    [RelayCommand]
    public async Task SendEnemyActionAsync()
    {
        if (!MasterMode || Combat == null || SelectedEnemy == null ||
            string.IsNullOrEmpty(SelectedActionType))
            return;

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

        ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "combat/enemy-move");
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();
        if (_listenTask != null) await _listenTask;
        _cts?.Dispose();
    }
}

public class CombatEvent
{
    public string EventType { get; set; }
    public Combat Combat { get; set; }
    public CombatLog Log { get; set; }
}