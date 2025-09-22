using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;

namespace DnDClient.ViewModels;

public partial class CreateNPCViewModel : ObservableObject
{
    [ObservableProperty] private string actionButtonText = "Создать";

    [ObservableProperty] private int armorClass;

    [ObservableProperty] private ObservableCollection<Attack> attacks = new();

    [ObservableProperty] private string bonds = string.Empty;

    [ObservableProperty] private string description = string.Empty;

    [ObservableProperty] private string flaws = string.Empty;

    [ObservableProperty] private int hitPoints;

    [ObservableProperty] private Guid id;

    [ObservableProperty] private string ideals = string.Empty;

    [ObservableProperty] private bool isEditMode;

    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private string occupation = string.Empty;

    [ObservableProperty] private string personalityTraits = string.Empty;

    [ObservableProperty] private string race = string.Empty;

    [ObservableProperty] private string role = string.Empty;

    [ObservableProperty] private string title = "Создать NPC";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("NPC", out var npc))
        {
            if (npc is NPC existingNpc)
            {
                IsEditMode = true;
                Title = "Редактировать NPC";
                ActionButtonText = "Сохранить";

                Id = existingNpc.Id;
                Name = existingNpc.Name;
                Description = existingNpc.Description;
                Race = existingNpc.Race;
                Occupation = existingNpc.Occupation;
                HitPoints = existingNpc.HitPoints;
                ArmorClass = existingNpc.ArmorClass;
                PersonalityTraits = existingNpc.PersonalityTraits;
                Ideals = existingNpc.Ideals;
                Bonds = existingNpc.Bonds;
                Flaws = existingNpc.Flaws;
                Attacks = new ObservableCollection<Attack>(existingNpc.Attacks ?? new ObservableCollection<Attack>());
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var npc = new NPC
            {
                Id = IsEditMode ? Id : Guid.NewGuid(),
                Name = Name,
                Description = Description,
                Race = Race,
                Occupation = Occupation,
                HitPoints = HitPoints,
                ArmorClass = ArmorClass,
                PersonalityTraits = PersonalityTraits,
                Ideals = Ideals,
                Bonds = Bonds,
                Flaws = Flaws,
                Attacks = Attacks
            };

            var json = JsonConvert.SerializeObject(npc);
            bool success;

            if (IsEditMode)
            {
                success = ApiHelper.Put<NPC>(json, "NPC", npc.Id);
            }
            else
            {
                success = ApiHelper.Post<NPC>(json, "NPC");
            }

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var action = IsEditMode ? "обновить" : "создать";
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось {action} NPC", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void AddAttack()
    {
        Attacks.Add(new Attack());
    }

    [RelayCommand]
    private void DelAttack(Attack attack)
    {
        if (attack != null)
        {
            if (attack.Id != Guid.Empty)
            {
                ApiHelper.Delete<Attack>("Attack", attack.Id);
            }

            Attacks.Remove(attack);
        }
    }
}