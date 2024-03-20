using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;

namespace TelegramBot.Handlers.FileEditing;

public class SavingResultsStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;
    
    public SavingResultsStateHandler(BotStorage botStorage, ILogger logger, MainMenu mainMenu)
    {
        _botStorage = botStorage;
        _logger = logger;
        _mainMenu = mainMenu;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered saving results state.");
        if (!message.Text.EndsWith(".csv") && !message.Text.EndsWith(".json"))
        {
            _logger.LogInformation($"{message.From.Id} [saving results state] bad file extension.");
            await botClient.SendTextMessageAsync(message.From.Id,
                "Название файла должно оканчиваться на \".csv\" или \".json\".");
            return;
        }

        var fileProcessor = new FileProcessorFactory(message.From.Id.ToString(),message.Text).CreateFileProcessor();
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        using var sr = new StreamReader(fileProcessor.Write(userInfo.CurIceHills));
        await System.IO.File.WriteAllTextAsync(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()), await sr.ReadToEndAsync());
        _logger.LogInformation($"{message.From.Id} [saving results state] result was saved successfully.");
        
        await botClient.SendTextMessageAsync(message.Chat.Id,"Результат сохранён!");
        userInfo.FileNames.Add(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()));
        
        await _mainMenu.EnterMainMenuAsync(botClient, message);
    }
}