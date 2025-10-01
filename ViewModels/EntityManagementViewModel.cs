using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;

namespace DnDClient.ViewModels;

public partial class EntityManagementViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Enemy> enemies = new();

    [ObservableProperty] private bool isEditingEnemy = false;

    [ObservableProperty] private bool isEditingNPC = false;

    [ObservableProperty] private Enemy newEnemy = new Enemy();

    [ObservableProperty] private NPC newNPC = new NPC();

    [ObservableProperty] private ObservableCollection<NPC> npcs = new();

    [ObservableProperty] private Enemy selectedEnemy;

    [ObservableProperty] private NPC selectedNPC;

    public EntityManagementViewModel()
    {
        LoadData();
    }

    public void LoadData()
    {
        Npcs = new ObservableCollection<NPC>(
            ApiHelper.Get<ObservableCollection<NPC>>("NPC") ?? new ObservableCollection<NPC>());

        Enemies = new ObservableCollection<Enemy>(
            ApiHelper.Get<ObservableCollection<Enemy>>("Enemy") ?? new ObservableCollection<Enemy>());
    }
    
    [RelayCommand]
    public void CreateNPC()
    {
        foreach (var attack in NewNPC.Attacks)
        {
            ApiHelper.Post<Attack>(Serdeser.Serialize(attack), "Attack");
        }
        var json = JsonConvert.SerializeObject(NewNPC);
        var success = ApiHelper.Post<NPC>(json, "NPC");

        if (success)
        {
            var result = ApiHelper.Get<NPC>("NPC", NewNPC.Id);
            if (result != null)
            {
                Npcs.Add(result);
                NewNPC = new NPC();
            }
        }
    }

    [RelayCommand]
    public void UpdateNPC()
    {
        if (SelectedNPC != null)
        {
            var json = JsonConvert.SerializeObject(SelectedNPC);
            ApiHelper.Put<NPC>(json, "NPC", SelectedNPC.Id);
            IsEditingNPC = false;
        }
    }

    [RelayCommand]
    public void EditNPC(NPC npc)
    {
        if (npc == null) return;
        SelectedNPC = npc;
        NewNPC = new NPC
        {
            Id = npc.Id,
            Name = npc.Name,
            Description = npc.Description,
            HitPoints = npc.HitPoints,
            ArmorClass = npc.ArmorClass,
            Attacks = new ObservableCollection<Attack>(npc.Attacks ?? new ObservableCollection<Attack>())
        };
        IsEditingNPC = true;
    }

    [RelayCommand]
    public void SaveNPCChanges()
    {
        foreach (var attack in NewNPC.Attacks)
        {
            ApiHelper.Put<Attack>(Serdeser.Serialize(attack), "Attack", attack.Id);
        }
        if (NewNPC == null) return;
        var json = JsonConvert.SerializeObject(NewNPC);
        var result = ApiHelper.Put<NPC>(json, "NPC", NewNPC.Id);
        if (result)
        {
            var index = Npcs.IndexOf(SelectedNPC);
            if (index != -1)
            {
                Npcs[index] = NewNPC;
            }

            IsEditingNPC = false;
            NewNPC = new NPC();
            LoadData();
        }
    }

    [RelayCommand]
    public void DeleteNPC(NPC npc)
    {
        if (npc == null) return;
        var result = ApiHelper.Delete<NPC>("NPC", npc.Id);
        if (result)
        {
            Npcs.Remove(npc);
        }
    }

    [RelayCommand]
    public async Task AddAttackToNPC()
    {
        var attack = new Attack();
        attack.NpcId = NewNPC.Id;
        NewNPC.Attacks = new();
        NewNPC.Attacks.Add(attack);
    }

    [RelayCommand]
    public void CreateEnemy()
    {
        foreach (var attack in NewEnemy.Attacks)
        {
            ApiHelper.Post<Attack>(Serdeser.Serialize(attack), "Attack");
        }
        
        var json = JsonConvert.SerializeObject(NewEnemy);
        var success = ApiHelper.Post<Enemy>(json, "Enemy");

        if (success)
        {
            var result = ApiHelper.Get<Enemy>("Enemy", NewEnemy.Id);
            if (result != null)
            {
                Enemies.Add(result);
                NewEnemy = new Enemy();
            }
        }
    }

    [RelayCommand]
    public void UpdateEnemy()
    {
        if (SelectedEnemy != null)
        {
            var json = JsonConvert.SerializeObject(SelectedEnemy);
            ApiHelper.Put<Enemy>(json, "Enemy", SelectedEnemy.Id);
            IsEditingEnemy = false;
        }
    }

    [RelayCommand]
    public void EditEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        SelectedEnemy = enemy;
        NewEnemy = new Enemy
        {
            Id = enemy.Id,
            Name = enemy.Name,
            Description = enemy.Description,
            CurrentHitPoints = enemy.CurrentHitPoints,
            ArmorClass = enemy.ArmorClass,
            SpecialAbilities = new ObservableCollection<SpecialAbility>(enemy.SpecialAbilities ?? new ObservableCollection<SpecialAbility>()),
            Attacks = new ObservableCollection<Attack>(enemy.Attacks ?? new ObservableCollection<Attack>())
        };
        IsEditingEnemy = true;
    }

    [RelayCommand]
    public void SaveEnemyChanges()
    {
        foreach (var attack in NewEnemy.Attacks)
        {
            ApiHelper.Put<Attack>(Serdeser.Serialize(attack), "Attack", attack.Id);
        }
        if (NewEnemy == null) return;
        var json = JsonConvert.SerializeObject(NewEnemy);
        var result = ApiHelper.Put<Enemy>(json, "Enemy", NewEnemy.Id);
        if (result)
        {
            var index = Enemies.IndexOf(SelectedEnemy);
            if (index != -1)
            {
                Enemies[index] = NewEnemy;
            }

            IsEditingEnemy = false;
            NewEnemy = new Enemy();
            LoadData();
        }
    }

    [RelayCommand]
    public void DeleteEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        var result = ApiHelper.Delete<Enemy>("Enemy", enemy.Id);
        if (result)
        {
            Enemies.Remove(enemy);
        }
    }

    [RelayCommand]
    public async Task AddAttackToEnemy()
    {
        var attack = new Attack();
        attack.EnemyId = NewEnemy.Id;
        NewEnemy.Attacks = new();
        NewEnemy.Attacks.Add(attack);
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (IsEditingNPC)
        {
            IsEditingNPC = false;
            NewNPC = new NPC();
        }

        if (IsEditingEnemy)
        {
            IsEditingEnemy = false;
            NewEnemy = new Enemy();
        }
    }

    public void AddSpecialAbilityToEnemy(SpecialAbility ability)
    {
        if (IsEditingEnemy && NewEnemy != null)
        {
            if (NewEnemy.SpecialAbilities == null)
                NewEnemy.SpecialAbilities = new ObservableCollection<SpecialAbility>();
            NewEnemy.SpecialAbilities.Add(ability);
        }
    }

    public void RemoveSpecialAbilityFromEnemy(SpecialAbility ability)
    {
        if (IsEditingEnemy && NewEnemy?.SpecialAbilities != null)
        {
            NewEnemy.SpecialAbilities.Remove(ability);
        }
    }
}