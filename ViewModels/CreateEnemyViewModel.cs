using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDClient.Models;
using DnDClient.Services;
using Newtonsoft.Json;

namespace DnDClient.ViewModels;

public partial class CreateEnemyViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] private string actionButtonText = "Создать";

    [ObservableProperty] private int? armorClass;

    [ObservableProperty] private ObservableCollection<Attack> attacks = new();

    [ObservableProperty] private string challengeRating = string.Empty;

    [ObservableProperty] private string description = string.Empty;

    [ObservableProperty] private int? hitPoints;

    [ObservableProperty] private Guid id;

    [ObservableProperty] private bool isEditMode;

    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private ObservableCollection<SpecialAbility> specialAbilities = new();

    [ObservableProperty] private string title = "Создать врага";

    [ObservableProperty] private string type = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Enemy", out var enemy))
        {
            if (enemy is Enemy existingEnemy)
            {
                IsEditMode = true;
                Title = "Редактировать врага";
                ActionButtonText = "Сохранить";

                Id = existingEnemy.Id;
                Name = existingEnemy.Name;
                Type = existingEnemy.Type ?? string.Empty;
                Description = existingEnemy.Description ?? string.Empty;
                HitPoints = existingEnemy.HitPoints;
                ArmorClass = existingEnemy.ArmorClass;
                ChallengeRating = existingEnemy.ChallengeRating ?? string.Empty;
                Attacks = new ObservableCollection<Attack>(existingEnemy.Attacks ?? new ObservableCollection<Attack>());
                SpecialAbilities = new ObservableCollection<SpecialAbility>(existingEnemy.SpecialAbilities ??
                                                                            new ObservableCollection<SpecialAbility>());
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var enemy = new Enemy
            {
                Id = IsEditMode ? Id : Guid.NewGuid(),
                Name = Name,
                Type = Type,
                Description = Description,
                HitPoints = HitPoints,
                ArmorClass = ArmorClass,
                ChallengeRating = ChallengeRating,
                Attacks = Attacks,
                SpecialAbilities = SpecialAbilities
            };

            var json = JsonConvert.SerializeObject(enemy);
            bool success;

            if (IsEditMode)
            {
                success = ApiHelper.Put<Enemy>(json, "Enemy", enemy.Id);
            }
            else
            {
                success = ApiHelper.Post<Enemy>(json, "Enemy");
            }

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var action = IsEditMode ? "обновить" : "создать";
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось {action} врага", "OK");
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

    [RelayCommand]
    private void AddAbility()
    {
        SpecialAbilities.Add(new SpecialAbility());
    }

    [RelayCommand]
    private void DelAbility(SpecialAbility ability)
    {
        if (ability != null)
        {
            if (ability.Id != Guid.Empty)
            {
                ApiHelper.Delete<SpecialAbility>("SpecialAbility", ability.Id);
            }

            SpecialAbilities.Remove(ability);
        }
    }
}