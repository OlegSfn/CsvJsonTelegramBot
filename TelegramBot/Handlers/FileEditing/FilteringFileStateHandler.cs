using Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers.FileEditing;

public class FilteringFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public FilteringFileStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }


    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().GetFilterValueKeyboard(userInfo).KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [filtered result] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        userInfo.CurIceHills = userInfo.CurIceHills.Where(x => x[userInfo.FieldToEdit] == message.Text).ToArray();
        await botClient.SendTextMessageAsync(message.Chat.Id,$"Найдено {userInfo.CurIceHills.Length} записей.\nВыберите следующий параметр или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
        _logger.LogInformation($"{message.From.Id} [filtered result] {userInfo.CurIceHills.Length} records.");
        userInfo.UserState = UserState.ChoosingFilter;
    }
}