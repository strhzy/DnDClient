using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Campaign : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StoryElement> plotItems = new();

    [ObservableProperty] 
    private ObservableCollection<PlayerCharacter> playerCharacters = new();

    [ObservableProperty]
    private ObservableCollection<Combat> combats = new();

    [ForeignKey("User")]
    [ObservableProperty]
    private Guid masterId;

    [JsonIgnore]
    [ObservableProperty]
    private User? master;
}