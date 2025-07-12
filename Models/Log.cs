using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class Log : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string tag = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private DateTime date = DateTime.UtcNow;
}
