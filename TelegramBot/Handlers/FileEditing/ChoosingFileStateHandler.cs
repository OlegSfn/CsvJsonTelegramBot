using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers.FileEditing;

public class ChoosingFileStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public ChoosingFileStateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;
    }
    
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered file choosing state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        
        if (!Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString()).KeyboardToVariants()
                .Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [file choosing state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }

        var fileProcessor = new FileProcessorFactory(message.From.Id.ToString(), message.Text).CreateFileProcessor();    
        userInfo.CurFileNameDB = message.Text.UserToDBFileName(message.From.Id.ToString());
        try
        {
            await using Stream stream = System.IO.File.OpenRead(userInfo.CurFileNameDB);
            userInfo.CurIceHills = fileProcessor.Read(stream);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите, что вы хотите сделать с файлом:",
                replyMarkup: Keyboards.GetInstance().FileEditModeKeyboard);
            userInfo.UserState = UserState.EditingFile;
            _logger.LogInformation($"{message.From.Id} [file choosing state] successfully read data from file.");
        }
        catch (IOException)
        {
            _logger.LogInformation($"{message.From.Id} [file choosing state] cannot read file.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Произошла ошибка при чтении из файла.");
        }
    }
}