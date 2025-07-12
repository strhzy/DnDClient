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
    private string name = string.Empty;
    
    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private int? attackBonus;
    
    [JsonPropertyName("damage_dice")]
    [ObservableProperty]
    private string damageDice = string.Empty;

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