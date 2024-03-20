using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

public class RegisterUserHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;
    
    public RegisterUserHandler(BotStorage botStorage, ILogger logger, MainMenu mainMenu)
    {
        _botStorage = botStorage;
        _logger = logger;
        _mainMenu = mainMenu;
    }
    
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        if (_botStorage.IdToUserInfoDict.ContainsKey(message.From.Id))
            _logger.LogInformation($"Found user {message.From.Id} in DB.");
        else
        {
            _logger.LogInformation($"Registered new user {message.From.Id} in DB.");
            var newUserInfo = new UserInfo();
            _botStorage.IdToUserInfoDict.Add(message.From.Id, newUserInfo);
        }
        
        await _mainMenu.EnterMainMenuAsync(botClient, message);
    }
}