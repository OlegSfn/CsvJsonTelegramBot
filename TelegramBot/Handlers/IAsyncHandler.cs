using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

public interface IAsyncHandler
{
    Task HandleAsync(ITelegramBotClient botClient, Message message);
}