using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class CombatParticipant : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int initiative;

    [ObservableProperty]
    private int currentHitPoints;

    [ObservableProperty]
    private int maxHitPoints;

    [ObservableProperty]
    private int armorClass;

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private ParticipantType type;

    [ObservableProperty]
    private Guid? sourceId;

    [ForeignKey("Combat")]
    [ObservableProperty]
    private Guid combatId;

    [JsonIgnore]
    [ObservableProperty]
    private Combat? combat;
}