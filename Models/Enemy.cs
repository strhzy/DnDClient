using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Enemy : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Attack> actions = new();

    [ObservableProperty] private string? alignment;

    [ObservableProperty] private int? armorClass;

    [ObservableProperty] private string? armorDescription;

    [ObservableProperty] private ObservableCollection<Attack> attacks;

    [ObservableProperty] private ObservableCollection<Attack> bonusActions = new();

    [ObservableProperty] private string? challengeRating;

    [ObservableProperty] private double? challengeRatingValue;

    [ObservableProperty] private int? charisma;

    [ObservableProperty] private string? conditionImmunities;

    [ObservableProperty] private int? constitution;

    [ObservableProperty] private string? damageImmunities;

    [ObservableProperty] private string? damageResistances;

    [ObservableProperty] private string? damageVulnerabilities;

    [ObservableProperty] private string? description;

    [ObservableProperty] private int? dexterity;

    [ObservableProperty] private string? group;

    [ObservableProperty] private string? hitDice;

    [ObservableProperty] private int? hitPoints;

    [Key] [ObservableProperty] private Guid id = Guid.NewGuid();

    [ObservableProperty] private int? intelligence;

    [ObservableProperty] private string? languages;

    [ObservableProperty] private ObservableCollection<Attack> legendaryActions = new();

    [ObservableProperty] private string? legendaryDescription;

    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int? perception;

    [ObservableProperty] private ObservableCollection<Attack> reactions = new();

    [ObservableProperty] private string? senses;

    [ObservableProperty] private string? size;

    [ObservableProperty] private Dictionary<string, int>? skills;

    [ObservableProperty] private string? slug;

    [ObservableProperty] private ObservableCollection<SpecialAbility> specialAbilities = new();

    [ObservableProperty] private Dictionary<string, string>? speed;

    [ObservableProperty] private ObservableCollection<string> spellObservableCollection = new();

    [ObservableProperty] private int? strength;

    [ObservableProperty] private string? subtype;

    [ObservableProperty] private string? type;

    [ObservableProperty] private int? wisdom;
}