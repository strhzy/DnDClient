using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using DnDClient.Views;
using Microsoft.AspNetCore.SignalR.Client;

namespace DnDClient.ViewModels;

public partial class CombatViewModel : ObservableObject, IAsyncDisposable
{
    private CancellationTokenSource? _cts;

    private HubConnection? _hubConnection;
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

    public async Task ConnectSignalRAsync()
    {
        if (Combat == null) return;

        _cts = new CancellationTokenSource();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"https://localhost:5228/api/combathub?combatId={Combat.Id}")
            .WithAutomaticReconnect()
            .Build();

        SetupMessageHandlers();

        try
        {
            await _hubConnection.StartAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task OpenCombatAsync(Combat combat)
    {
        if (combat == null) return;
        var navParam = new Dictionary<string, object>
        {
            { "Combat", combat },
            { "MasterMode", masterMode }
        };
        await Shell.Current.GoToAsync(nameof(CombatPage), navParam);
    }

    private void SetupMessageHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<CombatLog>("ReceivePlayerMove", log =>
        {
            if (MasterMode)
            {
                CombatLogs.Add(log);
            }
        });

        _hubConnection.On<Combat, CombatLog>("ReceiveMasterConfirm", (updatedCombat, log) =>
        {
            Combat = updatedCombat;
            if (!CombatLogs.Contains(log))
            {
                CombatLogs.Add(log);
            }
        });

        _hubConnection.On<Combat>("ReceiveCombatUpdate", updatedCombat => { Combat = updatedCombat; });

        _hubConnection.On<bool>("ReceiveTurnUpdate", isPlayerTurn => { IsPlayerTurn = isPlayerTurn; });

        _hubConnection.On<Combat, CombatLog>("ReceiveNpcMove", (updatedCombat, log) =>
        {
            Combat = updatedCombat;
            if (!CombatLogs.Contains(log))
            {
                CombatLogs.Add(log);
            }
        });

        _hubConnection.On<Combat, CombatLog>("ReceiveEnemyMove", (updatedCombat, log) =>
        {
            Combat = updatedCombat;
            if (!CombatLogs.Contains(log))
            {
                CombatLogs.Add(log);
            }
        });

        _hubConnection.Closed += async (error) =>
        {
            await Task.Delay(5000);
            await ConnectSignalRAsync();
        };

        _hubConnection.Reconnected += (connectionId) => { return Task.CompletedTask; };
    }

    [RelayCommand]
    public async Task SendPlayerActionAsync()
    {
        if (!IsPlayerTurn || Combat == null || SelectedTarget == null ||
            string.IsNullOrEmpty(SelectedActionType) || _hubConnection == null)
            return;

        var log = new CombatLog
        {
            Type = SelectedActionType,
            SourceId = Combat.CurrentParticipant.Id,
            TargetId = SelectedTarget.Id,
            Damage = AttackDamage,
            Message = $"Игрок {Combat.CurrentParticipant.Name} использует " +
                      $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget.Name}" +
                      $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
        };

        try
        {
            await _hubConnection.InvokeAsync("SendPlayerMove", log, _cts?.Token ?? default);
            ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "CombatLog");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ConfirmActionAsync(CombatLog log)
    {
        if (!MasterMode || Combat == null || _hubConnection == null) return;

        var source = Combat.Participants.FirstOrDefault(p => p.Id == log.SourceId);
        var target = Combat.Participants.FirstOrDefault(p => p.Id == log.TargetId);
        if (source == null || target == null) return;

        int damage = log.Damage ?? 0;
        if (log.Type == "attack")
            target.CurrentHitPoints = Math.Max(0, target.CurrentHitPoints - damage);
        else if (log.Type == "heal")
            target.CurrentHitPoints += damage;

        try
        {
            await _hubConnection.InvokeAsync("ConfirmAction", Combat, log, _cts?.Token ?? default);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Confirm error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task SendNpcActionAsync()
    {
        if (!MasterMode || Combat == null || SelectedNpc == null ||
            string.IsNullOrEmpty(SelectedActionType) || _hubConnection == null)
            return;

        var log = new CombatLog
        {
            Type = SelectedActionType,
            SourceId = SelectedNpc.Id,
            TargetId = SelectedTarget.Id,
            Damage = AttackDamage,
            Message = $"НПС {SelectedNpc.Name} использует " +
                      $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget?.Name ?? "цель"}" +
                      $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
        };

        try
        {
            await _hubConnection.InvokeAsync("SendNpcMove", log, _cts?.Token ?? default);
            ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "CombatLog");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send NPC error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task SendEnemyActionAsync()
    {
        if (!MasterMode || Combat == null || SelectedEnemy == null ||
            string.IsNullOrEmpty(SelectedActionType) || _hubConnection == null)
            return;

        var log = new CombatLog
        {
            Type = SelectedActionType,
            SourceId = SelectedEnemy.Id,
            TargetId = SelectedTarget.Id,
            Damage = AttackDamage,
            Message = $"Враг {SelectedEnemy.Name} использует " +
                      $"{SelectedAttack?.Name ?? SelectedActionType} на {SelectedTarget?.Name ?? "цель"}" +
                      $" ({(SelectedActionType == "attack" ? "урон" : "лечение")}: {AttackDamage})"
        };

        try
        {
            await _hubConnection.InvokeAsync("SendEnemyMove", log, _cts?.Token ?? default);
            ApiHelper.Post<CombatLog>(Serdeser.Serialize(log), "CombatLog");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send Enemy error: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ManageParticipants()
    {
        var vm = new CombatParticipantsViewModel(Combat);
        var page = new CombatParticipantsPage(Combat);
        Shell.Current.Navigation.PushAsync(page);
    }

    public ObservableCollection<CombatParticipant> GetNpcs()
    {
        return new ObservableCollection<CombatParticipant>(
            Combat.Participants.Where(p => p.Type == ParticipantType.Npc));
    }

    public ObservableCollection<CombatParticipant> GetEnemies()
    {
        return new ObservableCollection<CombatParticipant>(
            Combat.Participants.Where(p => p.Type == ParticipantType.Enemy));
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }

        _cts?.Cancel();
        _cts?.Dispose();
    }
}