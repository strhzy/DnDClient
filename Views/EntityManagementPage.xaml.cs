using DnDClient.ViewModels;

namespace DnDClient.Views;

public partial class EntityManagementPage : TabbedPage
{
    public EntityManagementPage()
    {
        InitializeComponent();
        BindingContext = new EntityManagementViewModel();
    }
}