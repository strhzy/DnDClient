using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Attack : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string name = "default";
    
    [ObservableProperty]
    private string description = "default";

    [ObservableProperty]
    private int? attackBonus;
    
    [JsonPropertyName("damage_dice")]
    [ObservableProperty]
    private string damageDice = "default";

    [ForeignKey("PlayerCharacter")]
    [ObservableProperty]
    private Guid? playerCharacterId;

    [ForeignKey("Enemy")]
    [ObservableProperty]
    private Guid? enemyId;

    [JsonIgnore]
    [ObservableProperty]
    private PlayerCharacter? playerCharacter;

    [JsonIgnore]
    [ObservableProperty]
    private Enemy? enemy;
}