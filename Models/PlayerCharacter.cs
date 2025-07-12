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
        private string name = string.Empty;

        [ObservableProperty]
        private string playerName = string.Empty;

        [ObservableProperty]
        private string classType = string.Empty;

        [ObservableProperty]
        private string background = string.Empty;

        [ObservableProperty]
        private string race = string.Empty;

        [ObservableProperty]
        private string alignment = string.Empty;

        [ObservableProperty]
        private int experiencePoints;

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
        private int copperPieces;

        [ObservableProperty]
        private int silverPieces;

        [ObservableProperty]
        private int electrumPieces;

        [ObservableProperty]
        private int goldPieces;

        [ObservableProperty]
        private int platinumPieces;

        // Черты характера
        [ObservableProperty]
        private string personalityTraits = string.Empty;

        [ObservableProperty]
        private string ideals = string.Empty;

        [ObservableProperty]
        private string bonds = string.Empty;

        [ObservableProperty]
        private string flaws = string.Empty;

        // Характеристики
        [ObservableProperty]
        private int strength;

        [ObservableProperty]
        private int dexterity;

        [ObservableProperty]
        private int constitution;

        [ObservableProperty]
        private int intelligence;

        [ObservableProperty]
        private int wisdom;

        [ObservableProperty]
        private int charisma;

        // Proficiency бонус
        [ObservableProperty]
        private int proficiencyBonus;

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
        private int savingThrowStrength;

        [ObservableProperty]
        private int savingThrowDexterity;

        [ObservableProperty]
        private int savingThrowConstitution;

        [ObservableProperty]
        private int savingThrowIntelligence;

        [ObservableProperty]
        private int savingThrowWisdom;

        [ObservableProperty]
        private int savingThrowCharisma;

        // Навыки
        [ObservableProperty]
        private int acrobatics;

        [ObservableProperty]
        private int animalHandling;

        [ObservableProperty]
        private int arcana;

        [ObservableProperty]
        private int athletics;

        [ObservableProperty]
        private int deception;

        [ObservableProperty]
        private int history;

        [ObservableProperty]
        private int insight;

        [ObservableProperty]
        private int intimidation;

        [ObservableProperty]
        private int investigation;

        [ObservableProperty]
        private int medicine;

        [ObservableProperty]
        private int nature;

        [ObservableProperty]
        private int perception;

        [ObservableProperty]
        private int performance;

        [ObservableProperty]
        private int persuasion;

        [ObservableProperty]
        private int religion;

        [ObservableProperty]
        private int sleightOfHand;

        [ObservableProperty]
        private int stealth;

        [ObservableProperty]
        private int survival;

        [ObservableProperty]
        private int passiveWisdom;

        // Боевая статистика
        [ObservableProperty]
        private int armorClass;

        [ObservableProperty]
        private int initiative;

        [ObservableProperty]
        private int speed;

        [ObservableProperty]
        private int maxHitPoints;

        [ObservableProperty]
        private int currentHitPoints;

        [ObservableProperty]
        private int temporaryHitPoints;

        [ObservableProperty]
        private string hitDice = string.Empty;

        [ObservableProperty]
        private int deathSaveSuccesses;

        [ObservableProperty]
        private int deathSaveFailures;

        // Атаки и заклинания
        [ObservableProperty]
        private List<Attack> attacks = new();

        [ObservableProperty]
        private string featuresAndTraits = string.Empty;

        [ObservableProperty]
        private string equipment = string.Empty;

        [ObservableProperty]
        private string proficienciesAndLanguages = string.Empty;

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