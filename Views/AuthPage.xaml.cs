using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class AuthPage : ContentPage
{
    public AuthPage()
    {
        InitializeComponent();
        BindingContext = new AuthViewModel();
    }
}