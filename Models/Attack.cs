using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Attack : ObservableObject
{
    [ObservableProperty] private int? attackBonus;

    [JsonPropertyName("damage_dice")] [ObservableProperty]
    private string damageDice = "default";

    [ObservableProperty] private string description = "default";

    [JsonIgnore] [ObservableProperty] private Enemy? enemy;

    [ForeignKey("Enemy")] [ObservableProperty]
    private Guid? enemyId;

    [Key] [ObservableProperty] private Guid id = Guid.NewGuid();

    [ObservableProperty] private string name = "default";

    [JsonIgnore] [ObservableProperty] private NPC? npc;

    [ForeignKey("NPC")] [ObservableProperty]
    private Guid? npcId;

    [JsonIgnore] [ObservableProperty] private PlayerCharacter? playerCharacter;

    [ForeignKey("PlayerCharacter")] [ObservableProperty]
    private Guid? playerCharacterId;
}