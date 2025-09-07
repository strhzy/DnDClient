using DnDClient.ViewModels;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using DnDClient.Models;
using Microsoft.Maui.Controls;

namespace DnDClient.Views;

public class ParticipantTypeToCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ParticipantType participantType && parameter is CombatParticipantsViewModel viewModel)
        {
            switch (participantType)
            {
                case ParticipantType.Player:
                    return viewModel.AvailableCharacters ?? new ObservableCollection<PlayerCharacter>();
                case ParticipantType.Npc:
                    return viewModel.AvailableNPCs ?? new ObservableCollection<NPC>();
                case ParticipantType.Enemy:
                    return viewModel.AvailableEnemies ?? new ObservableCollection<Enemy>();
                default:
                    return new ObservableCollection<object>();
            }
        }
        return new ObservableCollection<object>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}