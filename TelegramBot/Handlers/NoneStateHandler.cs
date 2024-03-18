using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;

namespace TelegramBot.Handlers;

public class NoneStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public NoneStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(message.From.Id, "Нажми /start, чтобы начать!");
        _logger.LogInformation($"{message.From.Id} asked to press /start.");
    }
}