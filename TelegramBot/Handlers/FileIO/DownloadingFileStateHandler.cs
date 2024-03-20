using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using Extensions;

namespace TelegramBot.Handlers.FileIO;

public class DownloadingFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;

    public DownloadingFileStateHandler(BotStorage botStorage, ILogger logger, MainMenu mainMenu)
    {
        _botStorage = botStorage;
        _logger = logger;
        _mainMenu = mainMenu;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered downloading state.");
        if (!Keyboards.GetInstance().DownloadModeKeyboard.KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [downloading state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        try
        {
            var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
            var fileProcessor = new FileProcessorFactory(message.From.Id.ToString(), $".{message.Text.ToLower()}").CreateFileProcessor();
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(
                    stream: fileProcessor.Write(userInfo.CurIceHills), 
                    fileName: Path.ChangeExtension(userInfo.CurFileNameDB.DBToUserFileName(message.From.Id.ToString()), Path.GetExtension(fileProcessor.FileName))),
                caption: "Ваш файл:");

            _logger.LogInformation($"{message.From.Id} [downloading state] successfully sent file.");
            await _mainMenu.EnterMainMenuAsync(botClient, message);
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation($"{message.From.Id} [downloading state] there is no such file.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
        }
    }
}