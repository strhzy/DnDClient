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

        _ = Task.Run(async () =>
        {
            await SseClient.ConnectAsync(
                $"http://strhzy.ru:5100/api/Combat/{Combat.Id}/stream",
                json =>
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("log", out var logElement))
                        {
                            var log = JsonSerializer.Deserialize<CombatLog>(logElement.GetRawText());
                            if (log != null)
                            {
                                MainThread.BeginInvokeOnMainThread(() => CombatLogs.Add(log));
                            }
                        }

                        if (doc.RootElement.TryGetProperty("combat", out var combatElement))
                        {
                            var newCombat = JsonSerializer.Deserialize<Combat>(combatElement.GetRawText());
                            if (newCombat != null)
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    Combat.Participants = newCombat.Participants;
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки JSON: {ex.Message}, JSON={json}");
                    }
                },
                _cts.Token
            );
        });
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