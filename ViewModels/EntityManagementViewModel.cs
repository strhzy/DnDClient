using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace DnDClient.ViewModels;

public partial class EntityManagementViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid campaignId;
    [ObservableProperty]
    private ObservableCollection<NPC> npcs = new();
    [ObservableProperty]
    private ObservableCollection<Enemy> enemies = new();

    [ObservableProperty]
    private NPC selectedNPC;
    [ObservableProperty]
    private Enemy selectedEnemy;

    [ObservableProperty]
    private NPC newNPC = new NPC();
    [ObservableProperty]
    private Enemy newEnemy = new Enemy();

    [ObservableProperty]
    private bool isEditingNPC = false;
    [ObservableProperty]
    private bool isEditingEnemy = false;

    public EntityManagementViewModel(Guid campaignId)
    {
        CampaignId = campaignId;
        LoadData();
    }

    public void LoadData()
    {
        Npcs = new ObservableCollection<NPC>(
            ApiHelper.Get<List<NPC>>("NPC") ?? new List<NPC>());

        Enemies = new ObservableCollection<Enemy>(
            ApiHelper.Get<List<Enemy>>("Enemy") ?? new List<Enemy>());
    }
    
    [RelayCommand]
    public async Task AddNpcToCampaignAsync(NPC npc)
    {
        if (npc == null) return;

        try
        {
            var result = ApiHelper.Post<string>($"", $"Campaign/{CampaignId}/add_npc/{npc.Id}");
            
            if (result != null)
            {
                await Shell.Current.DisplayAlert("Успех", $"NPC {npc.Name} добавлен в кампанию", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить NPC в кампанию", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Ошибка при добавлении NPC: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    public async Task AddEnemyToCampaignAsync(Enemy enemy)
    {
        if (enemy == null) return;

        try
        {
            var result = ApiHelper.Post<string>($"", $"Campaign/{CampaignId}/add_enemy/{enemy.Id}");
            
            if (result != null)
            {
                await Shell.Current.DisplayAlert("Успех", $"Враг {enemy.Name} добавлен в кампанию", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить врага в кампанию", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Ошибка при добавлении врага: {ex.Message}", "OK");
        }
    }

    // NPC CRUD operations
    [RelayCommand]
    public void CreateNPC()
    {
        var json = JsonConvert.SerializeObject(NewNPC);
        var success = ApiHelper.Post<NPC>(json, "NPC");
        
        if (success != null)
        {
            var result = ApiHelper.Get<NPC>("NPC", NewNPC.Id);
            Npcs.Add(result);
            NewNPC = new NPC();
        }
    }

    [RelayCommand]
    public void UpdateNPC()
    {
        if (SelectedNPC != null)
        {
            var json = JsonConvert.SerializeObject(SelectedNPC);
            ApiHelper.Put<NPC>(json, "NPC", selectedNPC.Id);
            IsEditingNPC = false;
        }
    }

    [RelayCommand]
    public void DeleteNPC(NPC npc)
    {
        if (npc != null)
        {
            ApiHelper.Delete<NPC>("NPC", npc.Id);
            Npcs.Remove(npc);
        }
    }

    [RelayCommand]
    public void EditNPC(NPC npc)
    {
        SelectedNPC = npc;
        IsEditingNPC = true;
    }

    [RelayCommand]
    public void CancelEditNPC()
    {
        IsEditingNPC = false;
        SelectedNPC = null;
        LoadData(); // Reload to discard changes
    }

    // Enemy CRUD operations
    [RelayCommand]
    public void CreateEnemy()
    {
        var json = JsonConvert.SerializeObject(NewEnemy);
        var success = ApiHelper.Post<Enemy>(json, "Enemy");
        
        if (success != null)
        {
            var result = ApiHelper.Get<Enemy>("Enemy", NewEnemy.Id);
            Enemies.Add(result);
            NewEnemy = new Enemy();
        }
    }

    [RelayCommand]
    public void UpdateEnemy()
    {
        if (SelectedEnemy != null)
        {
            var json = JsonConvert.SerializeObject(SelectedEnemy);
            ApiHelper.Put<Enemy>(json, "Enemy", selectedEnemy.Id);
            IsEditingEnemy = false;
        }
    }

    [RelayCommand]
    public void DeleteEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            ApiHelper.Delete<Enemy>("Enemy", enemy.Id);
            Enemies.Remove(enemy);
        }
    }

    [RelayCommand]
    public void EditEnemy(Enemy enemy)
    {
        SelectedEnemy = enemy;
        IsEditingEnemy = true;
    }

    [RelayCommand]
    public void CancelEditEnemy()
    {
        IsEditingEnemy = false;
        SelectedEnemy = null;
        LoadData(); // Reload to discard changes
    }
}