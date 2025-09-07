using DnDClient.ViewModels;
using Microsoft.Maui.Controls;

namespace DnDClient.Views
{
    public partial class CreateEnemyPage : ContentPage
    {
        public CreateEnemyPage()
        {
            InitializeComponent();
            BindingContext = new CreateEnemyViewModel();
        }
    }
}
