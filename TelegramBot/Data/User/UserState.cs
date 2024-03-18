namespace TelegramBot.Data.User;

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