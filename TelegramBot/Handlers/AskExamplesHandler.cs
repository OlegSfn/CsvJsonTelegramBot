using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents a handler for sending example files to the user.
/// </summary>
public class AskExamplesHandler : IAsyncHandler
{
    private readonly ILogger _logger;
    private readonly TransitionToMenuHandler _transitionToMenuHandler;
    
    public AskExamplesHandler(ILogger logger, TransitionToMenuHandler transitionToMenuHandler)
    {
        _logger = logger;
        _transitionToMenuHandler = transitionToMenuHandler;
    }
    
    public AskExamplesHandler() { }
    
    /// <summary>
    /// Handles the asynchronous message processing.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The received message.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"Sent examples to {message.From.Id}.");
        await using (var stream = System.IO.File.OpenRead(Path.Combine("../../../../", "data", "examples", "example.csv")))
        {
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(
                    stream: stream, 
                    fileName: "example.csv"
                ));
        }

        await using (var stream = System.IO.File.OpenRead(Path.Combine("../../../../", "data", "examples", "example.json")))
        {
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(
                    stream: stream, 
                    fileName: "example.json"
                ));
        }

        await _transitionToMenuHandler.HandleAsync(botClient, message);
    }
}