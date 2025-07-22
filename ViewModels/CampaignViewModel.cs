using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace DnDClient.ViewModels;

public partial class CampaignViewModel : ObservableObject
{
    [ObservableProperty] private Campaign campaign;

    [ObservableProperty] private bool masterMode;

    [ObservableProperty] private ObservableCollection<PlayerCharacter> players = new();
    [ObservableProperty] private ObservableCollection<PlayerCharacter> availableCharacters = new();
    [ObservableProperty] private PlayerCharacter selectedCharacterToAdd;
    [ObservableProperty] private ObservableCollection<StoryElement> stories = new();
    [ObservableProperty] private ObservableCollection<Combat> combats = new();

    [ObservableProperty] private string newStoryName;
    [ObservableProperty] private string newStoryDescription;

    public CampaignViewModel(Campaign _campaign)
    {
        campaign = _campaign;
        masterMode = Preferences.Get("current_user_id", "") == campaign.MasterId.ToString();
        LoadData();
    }

    public void LoadData()
    {
        Players = campaign.PlayerCharacters;
        var allChars = ApiHelper.Get<List<PlayerCharacter>>("PlayerCharacter") ?? new List<PlayerCharacter>();
        AvailableCharacters = new ObservableCollection<PlayerCharacter>(allChars.Except(Players, new PlayerCharacterIdComparer()));
        Stories = new ObservableCollection<StoryElement>(campaign.PlotItems);
        //Combats = new ObservableCollection<Combat>(ApiHelper.Get<List<Combat>>("campaigns/combats", Campaign.Id) ?? new List<Combat>());
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
            ApiHelper.Post<string>(json, "StoryElement");
            NewStoryName = string.Empty;
            NewStoryDescription = string.Empty;
            LoadData();
        }
    }

    private class PlayerCharacterIdComparer : IEqualityComparer<PlayerCharacter>
    {
        public bool Equals(PlayerCharacter x, PlayerCharacter y) => x.Id == y.Id;
        public int GetHashCode(PlayerCharacter obj) => obj.Id.GetHashCode();
    }
}