using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using DnDClient.Views;

namespace DnDClient.ViewModels;

public partial class CampaignListViewModel : ObservableObject
{
    private readonly INavigation _navigation;
    
    [ObservableProperty]
    private ObservableCollection<Campaign> campaigns;
    
    [RelayCommand]
    private async Task TapCard(Campaign campaign)
    {
        if (campaign != null)
        {
            await _navigation.PushAsync(new CampaignPage(campaign));
        }
    }
    
    public CampaignListViewModel(INavigation navigation)
    {
        _navigation = navigation;
        var userId = Preferences.Get("current_user_id", "");
        var userCampaigns = ApiHelper.Get<List<Campaign>>("Campaign?userId=" + userId) ?? new List<Campaign>();
        var masterCampaigns = ApiHelper.Get<List<Campaign>>("Campaign?masterId=" + userId) ?? new List<Campaign>();
        var allCampaigns = userCampaigns.Union(masterCampaigns, new CampaignIdComparer()).ToList();
        campaigns = new ObservableCollection<Campaign>(allCampaigns);
    }

    [RelayCommand]
    private async Task AddCampaign()
    {
        var campaign = new Campaign();
        var userId = Preferences.Get("current_user_id", "");
        campaign.MasterId = new Guid(userId);
        string json = Serdeser.Serialize(campaign);
        try
        {
            ApiHelper.Post<Campaign>(json, "Campaign");
            campaigns.Add(campaign);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [RelayCommand]
    private async Task DelCampaign(Campaign campaign)
    {
        try
        {
            ApiHelper.Delete<Campaign>("Campaign", campaign.Id);
            campaigns.Remove(campaign);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private class CampaignIdComparer : IEqualityComparer<Campaign>
    {
        public bool Equals(Campaign x, Campaign y) => x.Id == y.Id;
        public int GetHashCode(Campaign obj) => obj.Id.GetHashCode();
    }
}