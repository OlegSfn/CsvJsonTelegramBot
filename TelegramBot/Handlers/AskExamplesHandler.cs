using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

public class AskExamplesHandler : IAsyncHandler
{
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;
    
    public AskExamplesHandler(ILogger logger, MainMenu mainMenu)
    {
        _logger = logger;
        _mainMenu = mainMenu;
    }
    
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

        await _mainMenu.EnterMainMenuAsync(botClient, message);
    }
}