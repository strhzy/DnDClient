using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class CharactersPage : ContentPage
{
    public CharactersPage()
    {
        InitializeComponent();
        BindingContext = new CharactersViewModel(Navigation);
    }
}