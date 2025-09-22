using DnDClient.ViewModels;

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