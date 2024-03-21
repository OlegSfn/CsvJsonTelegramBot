using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents a handler for the 'None' state in the Telegram bot.
/// </summary>
public class NoneStateHandler : IAsyncHandler
{
    private readonly ILogger _logger;

    public NoneStateHandler(ILogger logger)
    {
        _logger = logger;
    }
    
    public NoneStateHandler() { }

    /// <summary>
    /// Handles incoming messages in the 'None' state.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(message.From.Id, "Нажми /start, чтобы начать!");
        _logger.LogInformation($"{message.From.Id} asked to press /start.");
    }
}