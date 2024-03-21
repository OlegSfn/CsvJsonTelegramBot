using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents an asynchronous handler for Telegram messages.
/// </summary>
public interface IAsyncHandler
{
    /// <summary>
    /// Handles the incoming Telegram message asynchronously.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    Task HandleAsync(ITelegramBotClient botClient, Message message);
}