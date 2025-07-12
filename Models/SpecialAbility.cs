using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class SpecialAbility : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? description;

    [ForeignKey("Enemy")]
    [ObservableProperty]
    private Guid enemyId;

    [JsonIgnore]
    [ObservableProperty]
    private Enemy? enemy;
}