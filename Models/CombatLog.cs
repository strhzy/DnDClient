using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class CombatLog : ObservableObject
{
    [Key] [ObservableProperty] private Guid id = Guid.NewGuid();
    [ForeignKey("Combat")] [ObservableProperty] private Guid? combatId;
    [ObservableProperty] private string type;
    [ObservableProperty] private Guid sourceId;
    [ObservableProperty] private Guid targetId;
    [ObservableProperty] private int? damage;
    [ObservableProperty] private string? message;
    [JsonIgnore] [ObservableProperty] private Combat? combat;
    [ObservableProperty] private CombatParticipant source;
    [ObservableProperty] private CombatParticipant target;
}