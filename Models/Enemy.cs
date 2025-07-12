using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Enemy : ObservableObject
    {
        [Key]
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        [ObservableProperty]
        private string? slug;
        
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private string? size;

        [ObservableProperty]
        private string? type;

        [ObservableProperty]
        private string? subtype;

        [ObservableProperty]
        private string? group;

        [ObservableProperty]
        private string? alignment;

        [ObservableProperty]
        private int? armorClass;
        
        [ObservableProperty]
        private string? armorDescription;

        [ObservableProperty]
        private int? hitPoints;

        [ObservableProperty]
        private string? hitDice;

        [ObservableProperty]
        private Dictionary<string, string>? speed;

        [ObservableProperty]
        private int? strength;

        [ObservableProperty]
        private int? dexterity;

        [ObservableProperty]
        private int? constitution;

        [ObservableProperty]
        private int? intelligence;

        [ObservableProperty]
        private int? wisdom;

        [ObservableProperty]
        private int? charisma;

        [ObservableProperty]
        private int? perception;

        [ObservableProperty]
        private Dictionary<string, int>? skills;

        [ObservableProperty]
        private string? damageVulnerabilities;

        [ObservableProperty]
        private string? damageResistances;

        [ObservableProperty]
        private string? damageImmunities;

        [ObservableProperty]
        private string? conditionImmunities;

        [ObservableProperty]
        private string? senses;

        [ObservableProperty]
        private string? languages;

        [ObservableProperty]
        private string? challengeRating;

        [ObservableProperty]
        private double? challengeRatingValue;

        [ObservableProperty]
        private List<Attack> actions = new();

        [ObservableProperty]
        private List<Attack> bonusActions = new();

        [ObservableProperty]
        private List<Attack> reactions = new();

        [ObservableProperty]
        private string? legendaryDescription;

        [ObservableProperty]
        private List<Attack> legendaryActions = new();

        [ObservableProperty]
        private List<SpecialAbility> specialAbilities = new();

        [ObservableProperty]
        private List<string> spellList = new();
    }