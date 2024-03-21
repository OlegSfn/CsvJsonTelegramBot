using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot;

/// <summary>
/// Represents the main menu of the Telegram bot.
/// </summary>
public class MainMenu
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public MainMenu(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

    /// <summary>
    /// Enters the main menu asynchronously.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message triggering the main menu.</param>
    public async Task EnterMainMenuAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered menu state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        await botClient.SendTextMessageAsync(message.Chat.Id, "Меню:", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
        userInfo.UserState = UserState.Menu;
    }
}