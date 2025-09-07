using System;
using System.Globalization;
using DnDClient.Models;
using Microsoft.Maui.Controls;

namespace DnDClient.Views;

public class HitPointsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CombatParticipant participant)
        {
            return $"{participant.CurrentHitPoints}/{participant.MaxHitPoints}";
        }
        return "0/0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}