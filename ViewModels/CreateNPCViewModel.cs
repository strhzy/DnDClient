using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.Services;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DnDClient.ViewModels
{
    public partial class CreateNPCViewModel : ObservableObject
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string role;
        [ObservableProperty] private string description;
        [ObservableProperty] private string race;
        [ObservableProperty] private string occupation;
        [ObservableProperty] private int hitPoints;
        [ObservableProperty] private int armorClass;
        [ObservableProperty] private string personalityTraits;
        [ObservableProperty] private string ideals;
        [ObservableProperty] private string bonds;
        [ObservableProperty] private string flaws;

        public CreateNPCViewModel()
        {
            
        }
        [RelayCommand]
        private async Task CreateNPCAsync()
        {
            var npc = new NPC
            {
                Name = Name,
                Description = Description,
                Race = Race,
                Occupation = Occupation,
                HitPoints = HitPoints,
                ArmorClass = ArmorClass,
                PersonalityTraits = PersonalityTraits,
                Ideals = Ideals,
                Bonds = Bonds,
                Flaws = Flaws
            };
            ApiHelper.Post<NPC>(Serdeser.Serialize(npc), "NPC");
            await Shell.Current.GoToAsync("..", true);
        }
    }
}
