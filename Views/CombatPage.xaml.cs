using DnDClient.Models;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class CombatPage : ContentPage
{
    public CombatPage(Combat combat, bool masterMode)
    {
        InitializeComponent();
        BindingContext = new CombatViewModel(combat, masterMode);
    }
}