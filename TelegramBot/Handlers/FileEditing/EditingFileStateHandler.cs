using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers.FileEditing;

/// <summary>
/// Represents a handler for processing the editing file state in the bot.
/// </summary>
public class EditingFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;

    public EditingFileStateHandler(BotStorage botStorage, ILogger logger, MainMenu mainMenu)
    {
        _botStorage = botStorage;
        _logger = logger;
        _mainMenu = mainMenu;
    }

    /// <summary>
    /// Handles the editing file state by processing the user's message.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered file editing state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (message.Text == "Отфильтровать")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] entering choosing filter state.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для выборки: ", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
            userInfo.UserState = UserState.ChoosingFilter;
        }
        else if (message.Text == "Отсортировать")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] entering choosing sorting state.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для сортировки: ", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            userInfo.UserState = UserState.ChoosingSortMode;
        }
        else if (message.Text == "Удалить")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] deleting chosen file.");
            userInfo.FileNames.Remove(userInfo.CurFileNameDB);
            System.IO.File.Delete(userInfo.CurFileNameDB);
            await botClient.SendTextMessageAsync(message.Chat.Id,"Файл успешно удалён.", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            await _mainMenu.EnterMainMenuAsync(botClient, message);
        }
        else if (message.Text == "Скачать")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] entering downloading state.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"В каком расширении скачать файл:", replyMarkup: Keyboards.GetInstance().DownloadModeKeyboard);
            userInfo.UserState = UserState.DownloadingFile;
        }
        else
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] got unexpected message.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.", replyMarkup: Keyboards.GetInstance().FileEditModeKeyboard);
        }
    }
}