using CsvHelper;
using CsvHelper.TypeConversion;
using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using File = System.IO.File;

namespace TelegramBot.Handlers.FileIO;

/// <summary>
/// Represents a handler for processing the new file upload state in the bot.
/// </summary>
public class NewFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly TransitionToMenuHandler _transitionToMenuHandler;

    public NewFileStateHandler(BotStorage botStorage, ILogger logger, TransitionToMenuHandler transitionToMenuHandler)
    {
        _botStorage = botStorage;
        _logger = logger;
        _transitionToMenuHandler = transitionToMenuHandler;
    }
    
    public NewFileStateHandler() { }

    /// <summary>
    /// Handles the new file upload state by processing the user's message.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
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
        
        
        var destinationFilePath = document.FileName.UserToDBFileName(message.From.Id.ToString());
        await using (Stream fileStream = File.Create(destinationFilePath))
        {
            await botClient.GetInfoAndDownloadFileAsync(
                fileId: document.FileId,
                destination: fileStream);
        }

        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        try
        {
            var fileProcessor = new FileProcessorFactory(message.From.Id.ToString(), document.FileName).CreateFileProcessor();
            IceHill[] iceHills;
            await using (Stream stream = File.OpenRead(destinationFilePath))
            {
                iceHills = fileProcessor.Read(stream); 
            }
            File.Delete(destinationFilePath);
            if (iceHills.Length == 0)
            {
                await botClient.SendTextMessageAsync(message.From.Id, "В файле нет данных о горках.\nОтправьте корректный файл или напишите /examples, чтобы посмотреть примеры файлов.");
                return;
            }
            
            fileProcessor = new FileProcessorFactory(message.From.Id.ToString(), ".csv").CreateFileProcessor();
            using (var sr = new StreamReader(fileProcessor.Write(iceHills)))
                await using (var sw = new StreamWriter(Path.ChangeExtension(destinationFilePath, ".csv")))
                    await sw.WriteAsync(await sr.ReadToEndAsync());
            
            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл успешно добавлен.");
            _logger.LogInformation($"{message.From.Id} [file upload state] file was saved successfully.");
            userInfo.FileNames.Add(Path.ChangeExtension(destinationFilePath, ".csv"));
            await _transitionToMenuHandler.HandleAsync(botClient, message);
        }
        catch (Exception e) when (e is BadDataException or ArgumentException or JsonSerializationException or JsonReaderException or ReaderException or TypeConverterException)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл некорректный.\nОтправьте корректный файл или напишите /examples, чтобы посмотреть примеры файлов.");
            _logger.LogInformation($"{message.From.Id} [file upload state] sent file with wrong data.");
            File.Delete(destinationFilePath);
        }
    }
}