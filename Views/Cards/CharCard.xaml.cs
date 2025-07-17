using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DnDClient.ViewModels;

namespace DnDClient.Views.Cards;

public partial class CharCard : ContentView
{
    public CharCard()
    {
        InitializeComponent();
        BindingContext = new CharactersViewModel(Navigation);
    }
}