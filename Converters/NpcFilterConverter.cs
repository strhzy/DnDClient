using System.Globalization;
using DnDClient.Models;

namespace DnDClient.Converters
{
    public class NpcFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<CombatParticipant> participants)
            {
                return participants.Where(p => p.Type == ParticipantType.Npc).ToList();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}