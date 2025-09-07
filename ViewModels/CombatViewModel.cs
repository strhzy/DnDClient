using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDClient.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using DnDClient.Views;
using System;
using Microsoft.Extensions.Logging;

namespace DnDClient.ViewModels;

public partial class CombatViewModel : ObservableObject, IAsyncDisposable
{
    [ObservableProperty] private bool masterMode;
    [ObservableProperty] private Combat combat;
    [ObservableProperty] private ObservableCollection<CombatLog> combatLogs = new();

    private HubConnection? _hubConnection;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string selectedActionType;
    [ObservableProperty] private CombatParticipant selectedTarget;
    [ObservableProperty] private Attack selectedAttack;
    [ObservableProperty] private bool isPlayerTurn;
    [ObservableProperty] private int attackDamage;
    
    public CombatViewModel(Combat _combat, bool _masterMode = false)
    {
        if (_combat != null)
        {
            Combat = _combat;
            MasterMode = _masterMode;
            IsPlayerTurn = !MasterMode && Combat.CurrentParticipant.Type == ParticipantType.Player;
        }
    }

    public async Task ConnectSignalRAsync()
    {
        if (Combat == null) return;

        _cts = new CancellationTokenSource();
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"https://localhost:5228/combathub?combatId={Combat.Id}")
            .WithAutomaticReconnect() // Автоматическое переподключение
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Information))
            .Build();

        // Регистрируем обработчики сообщений от сервера
        SetupMessageHandlers();

        try
        {
            await _hubConnection.StartAsync(_cts.Token);
            Console.WriteLine("Connected to SignalR Hub");
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
        await Shell.Current.GoToAsync(nameof(DnDClient.Views.CombatPage), navParam);
    }

    private void SetupMessageHandlers()
    {
        if (_hubConnection == null) return;

        // Обработка хода игрока (для мастера)
        _hubConnection.On<CombatLog>("ReceivePlayerMove", log =>
        {
            if (MasterMode)
            {
                // Добавляем лог в коллекцию для подтверждения мастером
                CombatLogs.Add(log);
            }
        });

        // Обработка подтверждения действия мастером
        _hubConnection.On<Combat, CombatLog>("ReceiveMasterConfirm", (updatedCombat, log) =>
        {
            Combat = updatedCombat;
            
            // Добавляем лог, если его еще нет
            if (!CombatLogs.Contains(log))
            {
                CombatLogs.Add(log);
            }
        });

        // Обработка обновления состояния боя
        _hubConnection.On<Combat>("ReceiveCombatUpdate", updatedCombat =>
        {
            Combat = updatedCombat;
        });

        // Обработка уведомления о ходе игрока
        _hubConnection.On<bool>("ReceiveTurnUpdate", isPlayerTurn =>
        {
            IsPlayerTurn = isPlayerTurn;
        });

        // Обработка событий подключения/отключения
        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine("Connection closed");
            await Task.Delay(5000);
            await ConnectSignalRAsync();
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            Console.WriteLine("Reconnected to hub");
            return Task.CompletedTask;
        };
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

        // Применяем изменения локально
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
    public async Task ManageParticipantsAsync()
    {
        var vm = new CombatParticipantsViewModel(Combat);
        var page = new CombatParticipantsPage(Combat);
        await Shell.Current.Navigation.PushAsync(page);
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

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}