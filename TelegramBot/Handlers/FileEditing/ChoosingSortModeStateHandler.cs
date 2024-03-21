using Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers.FileEditing;

/// <summary>
/// Represents a handler for processing the choosing sort mode state in the bot.
/// </summary>
public class ChoosingSortModeStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public ChoosingSortModeStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

    /// <summary>
    /// Handles the choosing sort mode state by processing the user's message.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered choosing sort mode state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().SortFieldKeyboard.KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "End")
        {
            _logger.LogInformation($"{message.From.Id} [choosing sort mode state] entering saving results state.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите режим сортировки: ", replyMarkup: Keyboards.GetInstance().SortModeKeyboard);
        _logger.LogInformation($"{message.From.Id} [choosing sort mode state] entering sorting state.");
        userInfo.UserState = UserState.SortingFile;
    }
}