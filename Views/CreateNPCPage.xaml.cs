using DnDClient.ViewModels;

namespace DnDClient.Views
{
    public partial class CreateNPCPage
    {
        public CreateNPCPage()
        {
            InitializeComponent();
            BindingContext = new CreateNPCViewModel();
        }
    }
}