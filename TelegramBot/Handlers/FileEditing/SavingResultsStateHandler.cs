using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;

namespace TelegramBot.Handlers.FileEditing;

/// <summary>
/// Represents a handler for processing the saving results state in the bot.
/// </summary>
public class SavingResultsStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly TransitionToMenuHandler _transitionToMenuHandler;
    
    public SavingResultsStateHandler(BotStorage botStorage, ILogger logger, TransitionToMenuHandler transitionToMenuHandler)
    {
        _botStorage = botStorage;
        _logger = logger;
        _transitionToMenuHandler = transitionToMenuHandler;
    }
    
    public SavingResultsStateHandler() { }

    /// <summary>
    /// Handles the saving results state by processing the user's message.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered saving results state.");
        var fileProcessor = new FileProcessorFactory(message.From.Id.ToString(),".csv").CreateFileProcessor();
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];

        try
        {
            using var sr = new StreamReader(fileProcessor.Write(userInfo.CurIceHills));
            await System.IO.File.WriteAllTextAsync(message.Text.UserToDBFileName(message.From.Id.ToString()),
                await sr.ReadToEndAsync());
            _logger.LogInformation($"{message.From.Id} [saving results state] result was saved successfully.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Результат сохранён!");
            userInfo.FileNames.Add(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()));

            await _transitionToMenuHandler.HandleAsync(botClient, message);
        }
        catch (IOException)
        {
            _logger.LogInformation($"{message.From.Id} [saving results state] result was not saved.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Недопустимое имя файла!");
        }

    }
}