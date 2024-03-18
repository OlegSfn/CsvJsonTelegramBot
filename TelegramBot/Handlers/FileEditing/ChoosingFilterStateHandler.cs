using Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

public class ChoosingFilterStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public ChoosingFilterStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered choosing filter state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().FilterFieldKeyboard.KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "End")
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] entering saving results state.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите значение для выборки: ", replyMarkup: Keyboards.GetInstance().GetFilterValueKeyboard(userInfo));
        _logger.LogInformation($"{message.From.Id} [choosing filter state] entering filtering state.");
        userInfo.UserState = UserState.FilteringFile;
    }
}