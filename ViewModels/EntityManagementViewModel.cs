using System.Collections.ObjectModel;
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

    // [ObservableProperty]
    // private Guid campaignId;
    [ObservableProperty] private ObservableCollection<NPC> npcs = new();

    [ObservableProperty] private Enemy selectedEnemy;

    [ObservableProperty] private NPC selectedNPC;

    public EntityManagementViewModel()
    {
        //CampaignId = campaignId;
        LoadData();
    }

    public void LoadData()
    {
        Npcs = new ObservableCollection<NPC>(
            ApiHelper.Get<List<NPC>>("NPC") ?? new List<NPC>());

        Enemies = new ObservableCollection<Enemy>(
            ApiHelper.Get<List<Enemy>>("Enemy") ?? new List<Enemy>());
    }

    // [RelayCommand]
    // public async Task AddNpcToCampaignAsync(NPC npc)
    // {
    //     if (npc == null) return;
    //
    //     try
    //     {
    //         var result = ApiHelper.Post<string>($"", $"Campaign/{CampaignId}/add_npc/{npc.Id}");
    //         
    //         if (result != null)
    //         {
    //             await Shell.Current.DisplayAlert("Успех", $"NPC {npc.Name} добавлен в кампанию", "OK");
    //         }
    //         else
    //         {
    //             await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить NPC в кампанию", "OK");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         await Shell.Current.DisplayAlert("Ошибка", $"Ошибка при добавлении NPC: {ex.Message}", "OK");
    //     }
    // }
    //
    // [RelayCommand]
    // public async Task AddEnemyToCampaignAsync(Enemy enemy)
    // {
    //     if (enemy == null) return;
    //
    //     try
    //     {
    //         var result = ApiHelper.Post<string>($"", $"Campaign/{CampaignId}/add_enemy/{enemy.Id}");
    //         
    //         if (result != null)
    //         {
    //             await Shell.Current.DisplayAlert("Успех", $"Враг {enemy.Name} добавлен в кампанию", "OK");
    //         }
    //         else
    //         {
    //             await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить врага в кампанию", "OK");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         await Shell.Current.DisplayAlert("Ошибка", $"Ошибка при добавлении врага: {ex.Message}", "OK");
    //     }
    // }

    // NPC CRUD operations
    [RelayCommand]
    public void CreateNPC()
    {
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
        };
        IsEditingNPC = true;
    }

    [RelayCommand]
    public void SaveNPCChanges()
    {
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

    // Enemy CRUD operations
    [RelayCommand]
    public void CreateEnemy()
    {
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
            HitPoints = enemy.HitPoints,
            ArmorClass = enemy.ArmorClass,
            SpecialAbilities =
                new ObservableCollection<SpecialAbility>(enemy.SpecialAbilities ??
                                                         new ObservableCollection<SpecialAbility>())
        };
        IsEditingEnemy = true;
    }

    [RelayCommand]
    public void SaveEnemyChanges()
    {
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