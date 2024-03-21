using IceHillProcessor;

namespace TelegramBot.Data.User;

/// <summary>
/// Represents information about a user.
/// </summary>
public class UserInfo
{
    public UserState UserState { get; set; }
    public string FieldToEdit { get; set; }
    public string CurFileNameDB { get; set; }
    public SortedSet<string> FileNames { get; set; } = new();
    public IceHill[] CurIceHills { get; set; } = Array.Empty<IceHill>();
}