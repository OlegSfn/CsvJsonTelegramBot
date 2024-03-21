using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents a handler for handling errors that occur during polling.
/// </summary>
public class ErrorHandler
{
    private readonly ILogger _logger;
    
    public ErrorHandler(ILogger logger)
    {
        _logger = logger;
    }
    
    public ErrorHandler() { }
    
    /// <summary>
    /// Handles polling errors from the Telegram bot.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="exception">The exception that occurred during polling.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}