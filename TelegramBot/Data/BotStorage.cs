using TelegramBot.Data.User;

namespace TelegramBot.Data;

/// <summary>
/// Represents the storage for the Telegram bot, mapping user IDs to user information.
/// </summary>
public class BotStorage
{
    public Dictionary<long, UserInfo> IdToUserInfoDict { get; } = new();
}