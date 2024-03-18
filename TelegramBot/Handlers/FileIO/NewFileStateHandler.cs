using CsvHelper;
using CsvHelper.TypeConversion;
using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using File = System.IO.File;

namespace TelegramBot.Handlers;

public class NewFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;

    public NewFileStateHandler(BotStorage botStorage, ILogger logger, MainMenu mainMenu)
    {
        _botStorage = botStorage;
        _logger = logger;
        _mainMenu = mainMenu;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered file upload state.");
        var document = message.Document;
        if (document == null)
        {
            _logger.LogInformation($"{message.From.Id} [file upload state] sent not file.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не прикрепили файл.");
            return;
        }

        if (Path.GetExtension(document.FileName) != ".csv" && Path.GetExtension(document.FileName) != ".json")
        {
            _logger.LogInformation($"{message.From.Id} [file upload state] sent file with wrong extension.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Расширение файла должно быть \".csv\" или \".json\".");
            return;
        }
        
        
        var destinationFilePath = PathExtensions.UserToDBFileName(document.FileName, message.From.Id.ToString());
        await using (Stream fileStream = File.Create(destinationFilePath))
        {
            await botClient.GetInfoAndDownloadFileAsync(
                fileId: document.FileId,
                destination: fileStream);
        }

        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        try
        {
            var fileProcessor = new FileProcessorFactory(document.FileName).CreateFileProcessor();
            await using Stream stream = File.OpenRead(destinationFilePath);
            fileProcessor.Read(stream); // Check for bad data.
            _logger.LogInformation($"{message.From.Id} [file upload state] file was saved successfully.");
        }
        catch (Exception e) when (e is BadDataException or ArgumentException or ReaderException or TypeConverterException)
        {
            _logger.LogInformation($"{message.From.Id} [file upload state] sent file with wrong data.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл некорректный.");
            File.Delete(destinationFilePath);
            return;
        }
        
        userInfo.FileNames.Add(destinationFilePath);
        await _mainMenu.EnterMainMenuAsync(botClient, message);
    }
}