using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class CombatParticipantsPage : TabbedPage
{
    public CombatParticipantsPage(Combat combat)
    {
        InitializeComponent();
        BindingContext = new CombatParticipantsViewModel(combat);
    }
}