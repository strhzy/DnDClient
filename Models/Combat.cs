using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Combat : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CombatParticipant> participants = new();

    [ObservableProperty]
    private int currentRound = 1;

    [ObservableProperty]
    private int currentTurnIndex = 0;

    [ForeignKey("Campaign")]
    [ObservableProperty]
    private Guid campaignId;

    [JsonIgnore]
    [ObservableProperty]
    private Campaign? campaign;

    [NotMapped]
    [ObservableProperty]
    private CombatParticipant? currentParticipant;
        
    partial void OnCurrentTurnIndexChanged(int value)
    {
        CurrentParticipant = Participants != null && value >= 0 && value < Participants.Count
            ? Participants[value]
            : null;
    }
}

public enum ParticipantType
{
    Player,
    Npc,
    Enemy
}