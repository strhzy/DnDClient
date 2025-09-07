using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace DnDClient.ViewModels;

public partial class CombatParticipantsViewModel : ObservableObject
{
    private INavigation _navigation;
    
    [ObservableProperty]
    private Combat combat;

    [ObservableProperty]
    private ObservableCollection<CombatParticipant> participants = new();

    [ObservableProperty]
    private ObservableCollection<PlayerCharacter> availableCharacters = new();
    
    [ObservableProperty]
    private ObservableCollection<NPC> availableNPCs = new();
    
    [ObservableProperty]
    private ObservableCollection<Enemy> availableEnemies = new();

    [ObservableProperty]
    private ParticipantType selectedParticipantType = ParticipantType.Player;

    [ObservableProperty] private object selectedEntityToAdd = new();

    public ObservableCollection<ParticipantType> ParticipantTypes { get; } = new ObservableCollection<ParticipantType>
    {
        ParticipantType.Player,
        ParticipantType.Npc,
        ParticipantType.Enemy
    };

    public CombatParticipantsViewModel(Combat _combat)
    {
        combat = _combat ?? throw new ArgumentNullException(nameof(_combat));
        Console.WriteLine($"CombatParticipantsViewModel initialized with Combat ID: {_combat?.Id}");
        combat.Participants ??= new ObservableCollection<CombatParticipant>();
        LoadData();
    }

    public void LoadData()
    {
        try
        {
            if (combat == null)
            {
                Console.WriteLine("Combat is null in LoadData");
                Participants = new ObservableCollection<CombatParticipant>();
                return;
            }

            Participants = combat.Participants != null 
                ? new ObservableCollection<CombatParticipant>(combat.Participants)
                : new ObservableCollection<CombatParticipant>();
            Console.WriteLine($"Loaded {Participants.Count} participants");
        
            LoadAvailableEntities();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
            Participants = new ObservableCollection<CombatParticipant>();
        }
    }

    private void LoadAvailableEntities()
    {
        try
        {
            availableCharacters = ApiHelper.Get<ObservableCollection<PlayerCharacter>>("PlayerCharacter");
            Console.WriteLine($"Loaded {availableCharacters.Count} PlayerCharacters");

            availableNPCs = ApiHelper.Get<ObservableCollection<NPC>>("NPC");
            Console.WriteLine($"Loaded {availableNPCs.Count} NPCs");

            availableEnemies = ApiHelper.Get<ObservableCollection<Enemy>>("Enemy");
            Console.WriteLine($"Loaded {availableEnemies.Count} Enemies");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading available entities: {ex.Message}");
            availableCharacters = new ObservableCollection<PlayerCharacter>();
            availableNPCs = new ObservableCollection<NPC>();
            availableEnemies = new ObservableCollection<Enemy>();
        }
    }

    [RelayCommand]
    public async Task AddParticipant(object entity)
    {
        if (entity == null)
        {
            Console.WriteLine("AddParticipant: Entity is null");
            return;
        }

        CombatParticipant participant = null;
        string successMessage = "";

        switch (entity)
        {
            case PlayerCharacter character:
                participant = CreateParticipantFromCharacter(character);
                successMessage = $"Персонаж {character.Name} добавлен в бой";
                break;
                
            case NPC npc:
                participant = CreateParticipantFromNPC(npc);
                successMessage = $"NPC {npc.Name} добавлен в бой";
                break;
                
            case Enemy enemy:
                participant = CreateParticipantFromEnemy(enemy);
                successMessage = $"Враг {enemy.Name} добавлен в бой";
                break;
        }

        if (participant != null)
        {
            try
            {
                var json = JsonConvert.SerializeObject(participant);
                var success = ApiHelper.Post<CombatParticipant>(json, $"Combat/{combat.Id}/participants");
                
                if (success)
                {
                    var result = ApiHelper.Get<CombatParticipant>("CombatParticipant", participant.Id);
                    if (result != null)
                    {
                        Participants.Add(result);
                        combat.Participants.Add(result);
                        await Shell.Current.DisplayAlert("Успех", successMessage, "OK");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve CombatParticipant with ID {participant.Id}");
                    }
                }
                else
                {
                    Console.WriteLine("API Post returned null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding participant: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось добавить участника: {ex.Message}", "OK");
            }
        }
    }

    private CombatParticipant CreateParticipantFromCharacter(PlayerCharacter character)
    {
        return new CombatParticipant
        {
            Name = character.Name,
            CurrentHitPoints = character.CurrentHitPoints,
            MaxHitPoints = character.MaxHitPoints,
            ArmorClass = character.ArmorClass,
            Initiative = 0,
            IsActive = false,
            Type = ParticipantType.Player,
            SourceId = character.Id,
            CombatId = combat.Id
        };
    }

    private CombatParticipant CreateParticipantFromNPC(NPC npc)
    {
        return new CombatParticipant
        {
            Name = npc.Name,
            CurrentHitPoints = npc.HitPoints,
            MaxHitPoints = npc.HitPoints,
            ArmorClass = npc.ArmorClass,
            Initiative = 0,
            IsActive = false,
            Type = ParticipantType.Npc,
            SourceId = npc.Id,
            CombatId = combat.Id
        };
    }

    private CombatParticipant CreateParticipantFromEnemy(Enemy enemy)
    {
        return new CombatParticipant
        {
            Name = enemy.Name,
            CurrentHitPoints = enemy.HitPoints ?? 0,
            MaxHitPoints = enemy.HitPoints ?? 0,
            ArmorClass = enemy.ArmorClass ?? 10,
            Initiative = 0,
            IsActive = false,
            Type = ParticipantType.Enemy,
            SourceId = enemy.Id,
            CombatId = combat.Id
        };
    }

    [RelayCommand]
    public void RemoveParticipant(CombatParticipant participant)
    {
        if (participant != null)
        {
            try
            {
                ApiHelper.Delete<CombatParticipant>("CombatParticipant", participant.Id);
                Participants.Remove(participant);
                combat.Participants.Remove(participant);
                Console.WriteLine($"Removed participant: {participant.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing participant: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("RemoveParticipant: Participant is null");
        }
    }

    [RelayCommand]
    public void UpdateParticipant(CombatParticipant participant)
    {
        if (participant != null)
        {
            try
            {
                var json = JsonConvert.SerializeObject(participant);
                ApiHelper.Put<CombatParticipant>(json, "CombatParticipant", participant.Id);
                Console.WriteLine($"Updated participant: {participant.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating participant: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("UpdateParticipant: Participant is null");
        }
    }

    [RelayCommand]
    public async Task OpenNPCCreation()
    {
        //await Shell.Current.GoToAsync(nameof(Views.CreateNPCPage));
        await _navigation.PushModalAsync(new Views.CreateNPCPage());
    }
    [RelayCommand]
    public async Task OpenEnemyCreation()
    {
        await _navigation.PushAsync(new Views.CreateEnemyPage());
    }
}