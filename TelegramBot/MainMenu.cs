using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot;

public class MainMenu
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public MainMenu(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

    public async Task EnterMainMenuAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered menu state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        await botClient.SendTextMessageAsync(message.Chat.Id, "Меню:", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
        userInfo.UserState = UserState.Menu;
    }
}