using CsvHelper;
using CsvHelper.TypeConversion;
using Extensions;
using IceHillProcessor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;
using File = System.IO.File;

namespace TelegramBot.Handlers;

public class UpdateHandler
{
    private readonly Dictionary<UserState, Func<ITelegramBotClient, Message, Task>> _stateHandlers;
    private readonly Dictionary<string, Func<ITelegramBotClient, Message, Task>> _commandsHandlers;
    private BotStorage _botStorage;
    private ILogger _logger;

    public UpdateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;

        _stateHandlers = new Dictionary<UserState, Func<ITelegramBotClient, Message, Task>>
        {
            { UserState.None, HandleNoneState},
            { UserState.Menu, HandleMenuState},
            { UserState.EnteringNewFile, HandleEnteringNewFileState},
            { UserState.EditingFile, HandleEditingFileState},
            { UserState.ChoosingFile, HandleChoosingFileState},
            { UserState.FilteringFile, HandleFilteringFileState},
            { UserState.SortingFile, HandleSortingFileState},
            { UserState.DownloadingFile, HandleDownloadingFileState},
            { UserState.ChoosingFilter, HandleChoosingFilterState},
            { UserState.ChoosingSortMode, HandleChoosingSortModeState},
            { UserState.SavingResults, HandleSavingResultsState}
        };
        _commandsHandlers = new Dictionary<string, Func<ITelegramBotClient, Message, Task>>
        {
            {"/start", HandleStart}
        };
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.From == null)
            return;
        
        _logger.LogInformation($"{update.Message.From.Id} sent: {update.Message.Text ?? "(no text)"}.");

        // Handle commands.
        if (_commandsHandlers.TryGetValue(update.Message.Text ?? "", out var handler))
        {
            await handler(botClient, update.Message);
            return;
        }
        
        // Handle messages.
        var user = _botStorage.IdToUserInfoDict[update.Message.From.Id];
        if (_stateHandlers.TryGetValue(user.UserState, out handler))
            await handler(botClient, update.Message);
        else
            _logger.LogInformation($"{update.Message.From.Id} unhandled state - {user.UserState}");
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
    
    private async Task HandleMenuState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (message.Text == "Enter new file")
        {
            _logger.LogInformation($"{message.From.Id} [menu state] entering new file.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Отправьте мне csv или json файл с данными.", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.EnteringNewFile;
        }
        else if (message.Text == "Edit file")
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
        else if (message.Text == "Download file")
        {
            if (userInfo.FileNames.Count == 0)
            {
                _logger.LogInformation($"{message.From.Id} [menu state] no files to download.");
                await botClient.SendTextMessageAsync(message.Chat.Id,"Вы не добавили ни один файл.", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
                return;
            }
            
            var replyKeyboardMarkup = Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString());
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите, какой файл нужно скачать:", replyMarkup: replyKeyboardMarkup);
            _logger.LogInformation($"{message.From.Id} [menu state] entering downloading file state.");
            userInfo.UserState = UserState.DownloadingFile;
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.");
        }
    }
    
    private async Task HandleEnteringNewFileState(ITelegramBotClient botClient, Message message)
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
        await EnterMainMenu(botClient, message);
    }

    private async Task HandleEditingFileState(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered file editing state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (message.Text == "Filter")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] entering choosing filter state.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для выборки: ", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
            userInfo.UserState = UserState.ChoosingFilter;
        }
        else if (message.Text == "Sort")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] entering choosing sorting state.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для сортировки: ", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            userInfo.UserState = UserState.ChoosingSortMode;
        }
        else if (message.Text == "Delete")
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] deleting chosen file.");
            userInfo.FileNames.Remove(userInfo.CurFileNameDB);
            File.Delete(userInfo.CurFileNameDB);
            await botClient.SendTextMessageAsync(message.Chat.Id,"Файл успешно удалён.", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            await EnterMainMenu(botClient, message);
        }
        else
        {
            _logger.LogInformation($"{message.From.Id} [file editing state] got unexpected message.");
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.", replyMarkup: Keyboards.GetInstance().FileEditModeKeyboard);
        }
    }

    private async Task HandleChoosingFileState(ITelegramBotClient botClient, Message message)
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

        if (!message.Text.EndsWith(".csv") && !message.Text.EndsWith(".json"))
        {
            _logger.LogInformation($"{message.From.Id} [file choosing state] wrong file extension.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Название файла должно заканчиваться на \".csv\" или \".json\".");
            return;
        }

        var fileProcessor = new FileProcessorFactory(message.Text).CreateFileProcessor();    
        userInfo.CurFileNameDB = PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString());
        try
        {
            await using Stream stream = File.OpenRead(userInfo.CurFileNameDB);
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
    
    private async Task HandleFilteringFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().GetFilterValueKeyboard(userInfo).KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [filtered result] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        userInfo.CurIceHills = userInfo.CurIceHills.Where(x => x[userInfo.FieldToEdit] == message.Text).ToArray();
        await botClient.SendTextMessageAsync(message.Chat.Id,$"Найдено {userInfo.CurIceHills.Length} записей.\nВыберите следующий параметр или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
        _logger.LogInformation($"{message.From.Id} [filtered result] {userInfo.CurIceHills.Length} records.");
        userInfo.UserState = UserState.ChoosingFilter;
    }
    
    private async Task HandleSortingFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        IEnumerable<IceHill> sortedCollection;
        if (userInfo.FieldToEdit == "ServicesWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x[userInfo.FieldToEdit]);
        else if (userInfo.FieldToEdit == "UsagePeriodWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x.OpenDate);
        else
        {
            _logger.LogInformation($"{message.From.Id} [sorted result] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "Ascending")
            userInfo.CurIceHills = sortedCollection.ToArray();
        else if (message.Text == "Descending")
            userInfo.CurIceHills = sortedCollection.Reverse().ToArray();
        else
        {
            _logger.LogInformation($"{message.From.Id} [sorted result] bad sort mode.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
            return;
        }

        _logger.LogInformation($"{message.From.Id} [sorted result] sorted successfully.");
        await botClient.SendTextMessageAsync(message.Chat.Id,"Сортировка успешно прошла. Выберите следующую сортировку или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
        userInfo.UserState = UserState.ChoosingSortMode;
    }
    
    private async Task HandleDownloadingFileState(ITelegramBotClient botClient, Message message)
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
            await using Stream stream = File.OpenRead(fileName);
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(stream: stream, fileName: message.Text),
                caption: "Ваш файл:");

            _logger.LogInformation($"{message.From.Id} [downloading state] successfully sent file.");
            await EnterMainMenu(botClient, message);
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation($"{message.From.Id} [downloading state] there is no such file.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
        }
    }
    
    private async Task HandleChoosingFilterState(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered choosing filter state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().FilterFieldKeyboard.KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "End")
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] entering saving results state.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите значение для выборки: ", replyMarkup: Keyboards.GetInstance().GetFilterValueKeyboard(userInfo));
        _logger.LogInformation($"{message.From.Id} [choosing filter state] entering filtering state.");
        userInfo.UserState = UserState.FilteringFile;
    }
    
    private async Task HandleChoosingSortModeState(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered choosing sort mode state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        if (!Keyboards.GetInstance().SortFieldKeyboard.KeyboardToVariants().Contains(message.Text))
        {
            _logger.LogInformation($"{message.From.Id} [choosing filter state] text is not from buttons.");
            await botClient.SendTextMessageAsync(message.From.Id, "Выберите один из вариантов.");
            return;
        }
        
        if (message.Text == "End")
        {
            _logger.LogInformation($"{message.From.Id} [choosing sort mode state] entering saving results state.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите режим сортировки: ", replyMarkup: Keyboards.GetInstance().SortModeKeyboard);
        _logger.LogInformation($"{message.From.Id} [choosing sort mode state] entering sorting state.");
        userInfo.UserState = UserState.SortingFile;
    }
    
    private async Task HandleSavingResultsState(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered saving results state.");
        if (!message.Text.EndsWith(".csv") && !message.Text.EndsWith(".json"))
        {
            _logger.LogInformation($"{message.From.Id} [saving results state] bad file extension.");
            await botClient.SendTextMessageAsync(message.From.Id,
                "Название файла должно оканчиваться на \".csv\" или \".json\".");
            return;
        }

        var fileProcessor = new FileProcessorFactory(message.Text).CreateFileProcessor();
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        using var sr = new StreamReader(fileProcessor.Write(userInfo.CurIceHills));
        await File.WriteAllTextAsync(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()), await sr.ReadToEndAsync());
        _logger.LogInformation($"{message.From.Id} [saving results state] result was saved successfully.");
        
        await botClient.SendTextMessageAsync(message.Chat.Id,"Результат сохранён!");
        userInfo.FileNames.Add(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()));
        
        await EnterMainMenu(botClient, message);
    }
    
    private async Task HandleStart(ITelegramBotClient botClient, Message message)
    {
        if (_botStorage.IdToUserInfoDict.ContainsKey(message.From.Id))
            _logger.LogInformation($"Found user {message.From.Id} in DB.");
        else
        {
            _logger.LogInformation($"Registered new user {message.From.Id} in DB.");
            var newUserInfo = new UserInfo();
            _botStorage.IdToUserInfoDict.Add(message.From.Id, newUserInfo);
        }
        
        await EnterMainMenu(botClient, message);
    }

    private async Task EnterMainMenu(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"{message.From.Id} entered menu state.");
        var userInfo = _botStorage.IdToUserInfoDict[message.From.Id];
        await botClient.SendTextMessageAsync(message.Chat.Id, "Меню:", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
        userInfo.UserState = UserState.Menu;
    }

    private async Task HandleNoneState(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(message.From.Id, "Нажми /start, чтобы начать!");
        _logger.LogInformation($"{message.From.Id} asked to press /start.");
    }
}