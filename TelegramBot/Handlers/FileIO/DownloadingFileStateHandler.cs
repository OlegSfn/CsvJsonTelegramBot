using Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;

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
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString()).KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [filtered result] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        var fileName = PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString());
        try
        {
            await using Stream stream = System.IO.File.OpenRead(fileName);
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(stream: stream, fileName: message.Text),
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