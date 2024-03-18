using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

public class SortingFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public SortingFileStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

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
        
        if (message.Text == "Ascending")
            userInfo.CurIceHills = sortedCollection.ToArray();
        else if (message.Text == "Descending")
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