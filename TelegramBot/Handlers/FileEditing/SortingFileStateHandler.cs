using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers.FileEditing;

/// <summary>
/// Represents a handler for processing the sorting file state in the bot.
/// </summary>
public class SortingFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public SortingFileStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }
    
    public SortingFileStateHandler() { }

    /// <summary>
    /// Handles the sorting file state by processing the user's message.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        IEnumerable<IceHill> sortedCollection;
        if (userInfo.FieldToEdit == "ServicesWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x[userInfo.FieldToEdit]);
        else if (userInfo.FieldToEdit == "UsagePeriodWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x.OpenDate);
        else
        {
            _logger.LogInformation($"{message.From.Id} [sorted result] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "По возрастанию")
            userInfo.CurIceHills = sortedCollection.ToArray();
        else if (message.Text == "По убыванию")
            userInfo.CurIceHills = sortedCollection.Reverse().ToArray();
        else
        {
            _logger.LogInformation($"{message.From.Id} [sorted result] bad sort mode.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
            return;
        }

        _logger.LogInformation($"{message.From.Id} [sorted result] sorted successfully.");
        await botClient.SendTextMessageAsync(message.Chat.Id,"Сортировка успешно прошла. Выберите следующую сортировку или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
        userInfo.UserState = UserState.ChoosingSortMode;
    }
}