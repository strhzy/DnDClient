using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDClient.Views;

public partial class CombatPage : ContentPage
{
    public CombatPage()
    {
        InitializeComponent();
        Loaded += CombatPage_Loaded;
    }

    private async void CombatPage_Loaded(object sender, EventArgs e)
    {
        if (BindingContext is DnDClient.ViewModels.CombatViewModel vm)
        {
            await vm.ConnectWebSocketAsync();
        }
    }
}