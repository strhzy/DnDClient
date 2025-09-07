using System.ComponentModel;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.Services;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DnDClient.ViewModels
{
    public partial class CreateEnemyViewModel : ObservableObject, INotifyPropertyChanged
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private string slug;
        [ObservableProperty] private string size;
        [ObservableProperty] private string type;
        [ObservableProperty] private string subtype;
        [ObservableProperty] private string group;
        [ObservableProperty] private string alignment;
        [ObservableProperty] private int armorClass;
        [ObservableProperty] private string armorDescription;
        [ObservableProperty] private int hitPoints;
        [ObservableProperty] private string hitDice;
        [ObservableProperty] private string speed;
        [ObservableProperty] private int strength;
        [ObservableProperty] private int dexterity;
        [ObservableProperty] private int constitution;
        [ObservableProperty] private int intelligence;
        [ObservableProperty] private int wisdom;
        [ObservableProperty] private int charisma;
        [ObservableProperty] private int perception;
        [ObservableProperty] private string skills;
        [ObservableProperty] private string damageVulnerabilities;
        [ObservableProperty] private string damageResistances;
        [ObservableProperty] private string damageImmunities;
        [ObservableProperty] private string conditionImmunities;
        [ObservableProperty] private string senses;
        [ObservableProperty] private string languages;
        [ObservableProperty] private string challengeRating;
        [ObservableProperty] private double challengeRatingValue;

        public CreateEnemyViewModel()
        {
            
        }
        
        [RelayCommand]
        private async Task CreateEnemyAsync()
        {
            var enemy = new Enemy
            {
                Name = Name,
                Description = Description,
                Slug = Slug,
                Size = Size,
                Type = Type,
                Subtype = Subtype,
                Group = Group,
                Alignment = Alignment,
                ArmorClass = ArmorClass,
                ArmorDescription = ArmorDescription,
                HitPoints = HitPoints,
                HitDice = HitDice,
                Strength = Strength,
                Dexterity = Dexterity,
                Constitution = Constitution,
                Intelligence = Intelligence,
                Wisdom = Wisdom,
                Charisma = Charisma,
                Perception = Perception,
                Senses = Senses,
                Languages = Languages,
                ChallengeRating = ChallengeRating,
                ChallengeRatingValue = ChallengeRatingValue,
                DamageVulnerabilities = DamageVulnerabilities,
                DamageResistances = DamageResistances,
                DamageImmunities = DamageImmunities,
                ConditionImmunities = ConditionImmunities
            };
            ApiHelper.Post<Enemy>(Serdeser.Serialize(enemy), "Enemy");
            await Shell.Current.GoToAsync("..", true);
        }
    }
}
