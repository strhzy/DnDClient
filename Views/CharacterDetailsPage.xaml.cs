using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class CharacterDetailsPage : ContentPage
{
    public CharacterDetailsPage(PlayerCharacter character)
    {
        InitializeComponent();
        BindingContext = new CharDetailsViewModel(Navigation, character);
    }
}