using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents a handler for registering users in the Telegram bot.
/// </summary>
public class RegisterUserHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    
    public RegisterUserHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }
    
    public RegisterUserHandler() { }
    
    /// <summary>
    /// Handles the registration of new users.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        if (_botStorage.IdToUserInfoDict.ContainsKey(message.From.Id))
            _logger.LogInformation($"Found user {message.From.Id} in DB.");
        else
        {
            _logger.LogInformation($"Registered new user {message.From.Id} in DB.");
            var newUserInfo = new UserInfo();
            _botStorage.IdToUserInfoDict.Add(message.From.Id, newUserInfo);
        }

        return Task.CompletedTask;
    }
}