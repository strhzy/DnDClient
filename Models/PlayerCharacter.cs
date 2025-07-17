using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class PlayerCharacter : ObservableObject
    {
        [Key]
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        [ObservableProperty]
        private string name = "default";

        [ObservableProperty]
        private string playerName = "default";

        [ObservableProperty]
        private string classType = "default";

        [ObservableProperty]
        private string background = "default";

        [ObservableProperty]
        private string race = "default";

        [ObservableProperty]
        private string alignment = "default";

        [ObservableProperty]
        private int experiencePoints = 0;

        [ObservableProperty]
        private int level = 1;

        [ObservableProperty]
        private bool inspiration;

        [ForeignKey("User")]
        [ObservableProperty]
        private Guid userId;

        [JsonIgnore]
        [ObservableProperty]
        private User? user;

        // Валюта
        [ObservableProperty]
        private int copperPieces = 0;

        [ObservableProperty]
        private int silverPieces = 0;

        [ObservableProperty]
        private int electrumPieces = 0;

        [ObservableProperty]
        private int goldPieces = 0;

        [ObservableProperty]
        private int platinumPieces = 0;

        // Черты характера
        [ObservableProperty]
        private string personalityTraits = "default";

        [ObservableProperty]
        private string ideals = "default";

        [ObservableProperty]
        private string bonds = "default";

        [ObservableProperty]
        private string flaws = "default";

        // Характеристики
        [ObservableProperty]
        private int strength = 0;

        [ObservableProperty]
        private int dexterity = 0;

        [ObservableProperty]
        private int constitution = 0;

        [ObservableProperty]
        private int intelligence = 0;

        [ObservableProperty]
        private int wisdom = 0;

        [ObservableProperty]
        private int charisma = 0;

        // Proficiency бонус
        [ObservableProperty]
        private int proficiencyBonus = 0;

        // Saving throws proficiencies
        [ObservableProperty]
        private bool savingThrowStrengthProficiency;

        [ObservableProperty]
        private bool savingThrowDexterityProficiency;

        [ObservableProperty]
        private bool savingThrowConstitutionProficiency;

        [ObservableProperty]
        private bool savingThrowIntelligenceProficiency;

        [ObservableProperty]
        private bool savingThrowWisdomProficiency;

        [ObservableProperty]
        private bool savingThrowCharismaProficiency;

        // Saving throws
        [ObservableProperty]
        private int savingThrowStrength = 0;

        [ObservableProperty]
        private int savingThrowDexterity = 0;

        [ObservableProperty]
        private int savingThrowConstitution = 0;

        [ObservableProperty]
        private int savingThrowIntelligence = 0;

        [ObservableProperty]
        private int savingThrowWisdom = 0;

        [ObservableProperty]
        private int savingThrowCharisma = 0;

        // Навыки
        [ObservableProperty]
        private int acrobatics = 0;

        [ObservableProperty]
        private int animalHandling = 0;

        [ObservableProperty]
        private int arcana = 0;

        [ObservableProperty]
        private int athletics = 0;

        [ObservableProperty]
        private int deception = 0;

        [ObservableProperty]
        private int history = 0;

        [ObservableProperty]
        private int insight = 0;

        [ObservableProperty]
        private int intimidation = 0;

        [ObservableProperty]
        private int investigation = 0;

        [ObservableProperty]
        private int medicine = 0;

        [ObservableProperty]
        private int nature = 0;

        [ObservableProperty]
        private int perception = 0;

        [ObservableProperty]
        private int performance = 0;

        [ObservableProperty]
        private int persuasion = 0;

        [ObservableProperty]
        private int religion = 0;

        [ObservableProperty]
        private int sleightOfHand = 0;

        [ObservableProperty]
        private int stealth = 0;

        [ObservableProperty]
        private int survival = 0;

        [ObservableProperty]
        private int passiveWisdom = 0;

        // Боевая статистика
        [ObservableProperty]
        private int armorClass = 0;

        [ObservableProperty]
        private int initiative = 0;

        [ObservableProperty]
        private int speed = 0;

        [ObservableProperty]
        private int maxHitPoints = 0;

        [ObservableProperty]
        private int currentHitPoints = 0;

        [ObservableProperty]
        private int temporaryHitPoints = 0;

        [ObservableProperty]
        private string hitDice = "default";

        [ObservableProperty]
        private int deathSaveSuccesses = 0;

        [ObservableProperty]
        private int deathSaveFailures = 0;

        // Атаки и заклинания
        [ObservableProperty]
        private List<Attack> attacks = new();

        [ObservableProperty]
        private string featuresAndTraits = "default";

        [ObservableProperty]
        private string equipment = "default";

        [ObservableProperty]
        private string proficienciesAndLanguages = "default";

        // Логика для saving throws
        private readonly Dictionary<string, (int Score, bool IsProficient, string SavingThrowProperty)> _abilityScores = new()
        {
            { nameof(Strength), (0, false, nameof(SavingThrowStrength)) },
            { nameof(Dexterity), (0, false, nameof(SavingThrowDexterity)) },
            { nameof(Constitution), (0, false, nameof(SavingThrowConstitution)) },
            { nameof(Intelligence), (0, false, nameof(SavingThrowIntelligence)) },
            { nameof(Wisdom), (0, false, nameof(SavingThrowWisdom)) },
            { nameof(Charisma), (0, false, nameof(SavingThrowCharisma)) }
        };

        private int CalculateSavingThrow(int abilityScore, bool isProficient)
        {
            int modifier = (abilityScore - 10) / 2;
            return isProficient ? modifier + ProficiencyBonus : modifier;
        }

        private void UpdateSavingThrow(string abilityName)
        {
            if (_abilityScores.TryGetValue(abilityName, out var data))
            {
                int value = CalculateSavingThrow(data.Score, data.IsProficient);
                typeof(PlayerCharacter).GetProperty(data.SavingThrowProperty)!.SetValue(this, value);
            }
        }

        partial void OnStrengthChanged(int value)
        {
            _abilityScores[nameof(Strength)] = (value, SavingThrowStrengthProficiency, nameof(SavingThrowStrength));
            UpdateSavingThrow(nameof(Strength));
        }

        partial void OnDexterityChanged(int value)
        {
            _abilityScores[nameof(Dexterity)] = (value, SavingThrowDexterityProficiency, nameof(SavingThrowDexterity));
            UpdateSavingThrow(nameof(Dexterity));
        }

        partial void OnConstitutionChanged(int value)
        {
            _abilityScores[nameof(Constitution)] = (value, SavingThrowConstitutionProficiency, nameof(SavingThrowConstitution));
            UpdateSavingThrow(nameof(Constitution));
        }

        partial void OnIntelligenceChanged(int value)
        {
            _abilityScores[nameof(Intelligence)] = (value, SavingThrowIntelligenceProficiency, nameof(SavingThrowIntelligence));
            UpdateSavingThrow(nameof(Intelligence));
        }

        partial void OnWisdomChanged(int value)
        {
            _abilityScores[nameof(Wisdom)] = (value, SavingThrowWisdomProficiency, nameof(SavingThrowWisdom));
            UpdateSavingThrow(nameof(Wisdom));
        }

        partial void OnCharismaChanged(int value)
        {
            _abilityScores[nameof(Charisma)] = (value, SavingThrowCharismaProficiency, nameof(SavingThrowCharisma));
            UpdateSavingThrow(nameof(Charisma));
        }

        partial void OnSavingThrowStrengthProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Strength)] = (Strength, value, nameof(SavingThrowStrength));
            UpdateSavingThrow(nameof(Strength));
        }

        partial void OnSavingThrowDexterityProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Dexterity)] = (Dexterity, value, nameof(SavingThrowDexterity));
            UpdateSavingThrow(nameof(Dexterity));
        }

        partial void OnSavingThrowConstitutionProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Constitution)] = (Constitution, value, nameof(SavingThrowConstitution));
            UpdateSavingThrow(nameof(Constitution));
        }

        partial void OnSavingThrowIntelligenceProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Intelligence)] = (Intelligence, value, nameof(SavingThrowIntelligence));
            UpdateSavingThrow(nameof(Intelligence));
        }

        partial void OnSavingThrowWisdomProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Wisdom)] = (Wisdom, value, nameof(SavingThrowWisdom));
            UpdateSavingThrow(nameof(Wisdom));
        }

        partial void OnSavingThrowCharismaProficiencyChanged(bool value)
        {
            _abilityScores[nameof(Charisma)] = (Charisma, value, nameof(SavingThrowCharisma));
            UpdateSavingThrow(nameof(Charisma));
        }

        partial void OnProficiencyBonusChanged(int value)
        {
            foreach (var key in _abilityScores.Keys)
            {
                UpdateSavingThrow(key);
            }
        }
    }