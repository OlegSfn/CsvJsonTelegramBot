using Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

public class MenuStateHandler : IAsyncHandler
{
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;
    private readonly HelpHandler _helpHandler;

    public MenuStateHandler(BotStorage botStorage, ILogger logger, HelpHandler helpHandler)
    {
        _botStorage = botStorage;
        _logger = logger;
        _helpHandler = helpHandler;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message) 
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().GetMainMenuKeyboard(userInfo).KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [menu state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "Добавить файл")
        {
            _logger.LogInformation($"{message.From.Id} [menu state] entering new file.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Отправьте мне csv или json файл с данными.", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.EnteringNewFile;
        }
        else if (message.Text == "Выбрать файл")
        {
            if (userInfo.FileNames.Count == 0)
            {
                _logger.LogInformation($"{message.From.Id} [menu state] no files to edit.");
                await botClient.SendTextMessageAsync(message.Chat.Id,"Вы не добавили ни один файл.", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
                return;
            }
                
            var replyKeyboardMarkup = Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString());
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите, какой файл нужно изменить:", replyMarkup: replyKeyboardMarkup);
            _logger.LogInformation($"{message.From.Id} [menu state] entering choosing file state.");
            userInfo.UserState = UserState.ChoosingFile;
        }
        else if (message.Text == "Помощь")
            await _helpHandler.HandleAsync(botClient, message);
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.");
        }
    }
}