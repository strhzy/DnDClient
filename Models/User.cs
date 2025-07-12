using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDClient.Models;

public partial class User : ObservableObject
{
    [Key]
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    
    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string passwordHash = string.Empty;

    [ObservableProperty]
    private UserRole role = UserRole.Player;

    [ObservableProperty]
    private List<PlayerCharacter> characters = new();
}

public enum UserRole
{
    Master,
    Player
}