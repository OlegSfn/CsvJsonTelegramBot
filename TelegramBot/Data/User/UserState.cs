namespace TelegramBot.Data.User;

/// <summary>
/// Represents the state of a user in the bot.
/// </summary>
public enum UserState
{
    None,
    Menu,
    EnteringNewFile,
    EditingFile,
    ChoosingFile,
    FilteringFile,
    SortingFile,
    DownloadingFile,
    ChoosingFilter,
    ChoosingSortMode,
    SavingResults
}