using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class CombatPage : ContentPage
{
    public CombatPage(Combat combat, bool masterMode)
    {
        InitializeComponent();
        BindingContext = new CombatViewModel(combat, masterMode);
        Loaded += CombatPage_Loaded;
    }

    private async void CombatPage_Loaded(object sender, EventArgs e)
    {
        if (BindingContext is DnDClient.ViewModels.CombatViewModel vm)
        {
            await vm.ConnectSignalRAsync();
        }
    }
}