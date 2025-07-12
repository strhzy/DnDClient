using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class StoryElement : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ForeignKey("Campaign")]
    [ObservableProperty]
    private Guid campaignId;

    [JsonIgnore]
    [ObservableProperty]
    private Campaign? campaign;
}