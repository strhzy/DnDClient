using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using DnDClient.Views;

namespace DnDClient.ViewModels;

public partial class CampaignViewModel : ObservableObject
{
    private readonly INavigation _navigation;
    
    [ObservableProperty] private Campaign campaign;

    [ObservableProperty] private bool masterMode;

    [ObservableProperty] private ObservableCollection<PlayerCharacter> players = new();
    [ObservableProperty] private ObservableCollection<PlayerCharacter> availableCharacters = new();
    [ObservableProperty] private PlayerCharacter selectedCharacterToAdd;
    [ObservableProperty] private ObservableCollection<StoryElement> stories = new();
    [ObservableProperty] private ObservableCollection<Combat> combats = new();
    [ObservableProperty] private ObservableCollection<PlayerCharacter> filtredCharacters = new();

    [ObservableProperty] private string newStoryName;
    [ObservableProperty] private string newStoryDescription;
    [ObservableProperty] private string newCombatName;

    public CampaignViewModel(Campaign? _campaign)
    {
        if (_campaign != null)
        {
            _navigation = Application.Current.MainPage.Navigation;
            campaign = _campaign;
            masterMode = Preferences.Get("current_user_id", "") == campaign.MasterId.ToString();
            LoadData();
        }
    }

    public void LoadData()
    {
        Players = campaign.PlayerCharacters;
        var allChars = ApiHelper.Get<List<PlayerCharacter>>("PlayerCharacter") ?? new List<PlayerCharacter>();
        AvailableCharacters = new ObservableCollection<PlayerCharacter>(allChars.Except(Players, new PlayerCharacterIdComparer()));
        Stories = ApiHelper.Get<ObservableCollection<StoryElement>>("StoryElement?campaignId="+Campaign.Id.ToString());
        Combats = ApiHelper.Get<ObservableCollection<Combat>>("Combat?campaignId="+Campaign.Id.ToString());
        Campaign.Combats = Combats;
        Campaign.PlotItems = Stories;
    }

    public void AddPlayerToCampaign()
    {
        if (SelectedCharacterToAdd != null)
        {
            var json = JsonConvert.SerializeObject(SelectedCharacterToAdd.Id);
            ApiHelper.Post<string>("", $"Campaign/{Campaign.Id}/add_char/{SelectedCharacterToAdd.Id}");
            LoadData();
        }
    }

    [RelayCommand]
    public void SaveCampaign()
    {
        ApiHelper.Put<Campaign>(Serdeser.Serialize(Campaign), "Campaign", Campaign.Id);
    }

    [RelayCommand]
    public void AddStory()
    {
        if (!string.IsNullOrWhiteSpace(NewStoryName))
        {
            var story = new StoryElement
            {
                Name = NewStoryName,
                Description = NewStoryDescription,
                CampaignId = Campaign.Id
            };
            var json = JsonConvert.SerializeObject(story);
            ApiHelper.Post<StoryElement>(json, "StoryElement");
            NewStoryName = string.Empty;
            NewStoryDescription = string.Empty;
            LoadData();
        }
    }
    
    [RelayCommand]
    public void AddCombat()
    {
        var combat = new Combat
        {
            Name = NewCombatName,
            CampaignId = campaign.Id,
            Campaign = campaign,
            Participants = new ObservableCollection<CombatParticipant>()
        };
        var json = JsonConvert.SerializeObject(combat);
        bool success = ApiHelper.Post<Combat>(json, "Combat");
        LoadData();
    }

    [RelayCommand]
    public void DeleteStory(StoryElement story)
    {
        if (story != null)
        {
            ApiHelper.Delete<StoryElement>($"StoryElement", story.Id);
            LoadData();
        }
    }

    [RelayCommand]
    public void DeleteCombat(Combat combat)
    {
        if (combat != null)
        {
            ApiHelper.Delete<Combat>("Combat", combat.Id);
            LoadData();
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
        await _navigation.PushAsync(new CombatPage(combat, masterMode));
    }
    
    [RelayCommand]
    public async Task ManageCombatParticipants(Combat combat)
    {
        if (combat != null)
        {
            await _navigation.PushAsync(new CombatParticipantsPage(combat));
        }
    }

    private class PlayerCharacterIdComparer : IEqualityComparer<PlayerCharacter>
    {
        public bool Equals(PlayerCharacter x, PlayerCharacter y) => x.Id == y.Id;
        public int GetHashCode(PlayerCharacter obj) => obj.Id.GetHashCode();
    }
}