using CsvHelper;
using Extensions;
using IceHillProcessor;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;
using File = System.IO.File;

namespace TelegramBot.Handlers;

public class UpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null)
            return;
        
        var userState = UserState.None;
        if (BotStorage.GetInstance().IdToUserInfoDict.ContainsKey(update.Message.From.Id))
            userState = BotStorage.GetInstance().IdToUserInfoDict[update.Message.From.Id].UserState;

        if (update.Type == UpdateType.Message)
        {
            if (update.Message.Text == "/start")
                await HandleStart(botClient, update.Message);
            else
            {
                switch (userState)
                {
                    case UserState.None:
                        await botClient.SendTextMessageAsync(update.Message.From.Id, "Нажми /start, чтобы начать!");
                        break;
                    case UserState.Menu:
                        await HandleMenuState(botClient, update.Message);
                        break;
                    case UserState.EnteringNewFile:
                        await HandleEnteringNewFileState(botClient, update.Message);
                        break;
                    case UserState.EditingFile:
                        await HandleEditingFileState(botClient, update.Message);
                        break;
                    case UserState.ChoosingFile:
                        await HandleChoosingFileState(botClient, update.Message);
                        break;
                    case UserState.FilteringFile:
                        await HandleFilteringFileState(botClient, update.Message);
                        break;
                    case UserState.SortingFile:
                        await HandleSortingFileState(botClient, update.Message);
                        break;
                    case UserState.DownloadingFile:
                        await HandleDownloadingFileState(botClient, update.Message);
                        break;
                    case UserState.ChoosingFilter:
                        await HandleChoosingFilterState(botClient, update.Message);
                        break;
                    case UserState.ChoosingSortMode:
                        await HandleChoosingSortModeState(botClient, update.Message);
                        break;
                    case UserState.SavingResults:
                        await HandleSavingResultsState(botClient, update.Message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private async Task HandleMenuState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        if (message.Text == "Enter new file")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Отправьте мне csv или json файл с данными.", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.EnteringNewFile;
        }
        else if (message.Text == "Edit file")
        {
            if (userInfo.FileNames.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,"Вы не добавили ни один файл.", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
                return;
            }
            
            var replyKeyboardMarkup = Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString());
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите, какой файл нужно изменить:", replyMarkup: replyKeyboardMarkup);
            userInfo.UserState = UserState.ChoosingFile;
        }
        else if (message.Text == "Download file")
        {
            if (userInfo.FileNames.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,"Вы не добавили ни один файл.", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
                return;
            }
            
            var replyKeyboardMarkup = Keyboards.GetInstance().GetFileNamesKeyboard(userInfo, message.From.Id.ToString());
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите, какой файл нужно скачать:", replyMarkup: replyKeyboardMarkup);
            userInfo.UserState = UserState.DownloadingFile;
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.");
        }
    }
    
    private async Task HandleEnteringNewFileState(ITelegramBotClient botClient, Message message)
    {
        var document = message.Document;
        if (document == null)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не прикрепили файл.");
            return;
        }

        if (Path.GetExtension(document.FileName) != ".csv" && Path.GetExtension(document.FileName) != ".json")
        {
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

        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        try
        {
            IFileProcessor fileProcessor;
            if (document.FileName.EndsWith(".csv"))
                fileProcessor = new CSVProcessing();
            else if (document.FileName.EndsWith(".json"))
                fileProcessor = new JSONProcessing();
            else
                return;

            await using Stream stream = File.OpenRead(destinationFilePath);
            fileProcessor.Read(stream); // Check for bad data.
        }
        catch (Exception e) when (e is BadDataException or ArgumentException)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл некорректный.");
            File.Delete(destinationFilePath);
            return;
        }
        
        userInfo.FileNames.Add(destinationFilePath);
        await EnterMainMenu(botClient, message);
    }

    private async Task HandleEditingFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        if (message.Text == "Filter")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для выборки: ", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
            userInfo.UserState = UserState.ChoosingFilter;
        }
        else if (message.Text == "Sort")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите поле для сортировки: ", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            userInfo.UserState = UserState.ChoosingSortMode;
        }
        else if (message.Text == "Delete")
        {
            userInfo.FileNames.Remove(userInfo.CurFileNameDB);
            File.Delete(userInfo.CurFileNameDB);
            await botClient.SendTextMessageAsync(message.Chat.Id,"Файл успешно удалён.", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
            await EnterMainMenu(botClient, message);
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите один из вариантов.", replyMarkup: Keyboards.GetInstance().FileEditModeKeyboard);
        }
    }

    private async Task HandleChoosingFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        IFileProcessor fileProcessor;
        if (message.Text.EndsWith(".csv"))
            fileProcessor = new CSVProcessing();
        else if (message.Text.EndsWith(".json"))
            fileProcessor = new JSONProcessing();
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Название файла должно заканчиваться на \".csv\" или \".json\".");
            return;
        }

        userInfo.CurFileNameDB = PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString());

        try
        {
            await using Stream stream = File.OpenRead(userInfo.CurFileNameDB);
            userInfo.CurIceHills = fileProcessor.Read(stream);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите, что вы хотите сделать с файлом:",
                replyMarkup: Keyboards.GetInstance().FileEditModeKeyboard);
            userInfo.UserState = UserState.EditingFile;
        }
        catch (FileNotFoundException)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
        }
    }
    
    private async Task HandleFilteringFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        userInfo.CurIceHills = userInfo.CurIceHills.Where(x => x[userInfo.FieldToEdit] == message.Text).ToArray();
        await botClient.SendTextMessageAsync(message.Chat.Id,$"Найдено {userInfo.CurIceHills.Length} записей.\nВыберите следующий параметр или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().FilterFieldKeyboard);
        userInfo.UserState = UserState.ChoosingFilter;
    }
    
    private async Task HandleSortingFileState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        IEnumerable<IceHill> sortedCollection;
        if (userInfo.FieldToEdit == "ServicesWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x[userInfo.FieldToEdit]);
        else if (userInfo.FieldToEdit == "UsagePeriodWinter")
            sortedCollection = userInfo.CurIceHills.OrderBy(x => x.OpenDate);
        else
            sortedCollection = userInfo.CurIceHills;
        
        if (message.Text == "Ascending")
            userInfo.CurIceHills = sortedCollection.ToArray();
        else if (message.Text == "Descending")
            userInfo.CurIceHills = sortedCollection.Reverse().ToArray();
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
            return;
        }

        await botClient.SendTextMessageAsync(message.Chat.Id,"Сортировка успешно прошла. Выберите следующую сортировку или \"End\", чтобы закончить:", replyMarkup: Keyboards.GetInstance().SortFieldKeyboard);
        userInfo.UserState = UserState.ChoosingSortMode;
    }
    
    private async Task HandleDownloadingFileState(ITelegramBotClient botClient, Message message)
    {
        var fileName = PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString());
        try
        {
            await using Stream stream = File.OpenRead(fileName);
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(stream: stream, fileName: message.Text),
                caption: "Ваш файл:");

            await EnterMainMenu(botClient, message);
        }
        catch (FileNotFoundException)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите один из вариантов.");
        }
    }
    
    private async Task HandleChoosingFilterState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        if (message.Text == "End")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите значение для выборки: ", replyMarkup: Keyboards.GetInstance().GetFilterValueKeyboard(userInfo));
        userInfo.UserState = UserState.FilteringFile;
    }
    
    private async Task HandleChoosingSortModeState(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        if (message.Text == "End")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название файла с расширением, куда сохранить результат:", replyMarkup: new ReplyKeyboardRemove());
            userInfo.UserState = UserState.SavingResults;
            return;
        }
        
        userInfo.FieldToEdit = message.Text;
        await botClient.SendTextMessageAsync(message.Chat.Id,"Выберите режим сортировки: ", replyMarkup: Keyboards.GetInstance().SortModeKeyboard);
        userInfo.UserState = UserState.SortingFile;
    }
    
    private async Task HandleSavingResultsState(ITelegramBotClient botClient, Message message)
    {
        IFileProcessor fileProcessor;
        if (message.Text.EndsWith(".csv"))
            fileProcessor = new CSVProcessing();
        else if (message.Text.EndsWith(".json"))
            fileProcessor = new JSONProcessing();
        else
        {
            await botClient.SendTextMessageAsync(message.From.Id,
                "Название файла должно оканчиваться на \".csv\" или \".json\".");
            return;
        }

        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        using var sr = new StreamReader(fileProcessor.Write(userInfo.CurIceHills));
        await File.WriteAllTextAsync(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()), await sr.ReadToEndAsync());
        await botClient.SendTextMessageAsync(message.Chat.Id,"Результат сохранён!");
        userInfo.FileNames.Add(PathExtensions.UserToDBFileName(message.Text, message.From.Id.ToString()));
        await EnterMainMenu(botClient, message);
    }
    
    private async Task HandleStart(ITelegramBotClient botClient, Message message)
    {
        var botStorage = BotStorage.GetInstance();
        if (botStorage.IdToUserInfoDict.ContainsKey(message.From.Id))
        {
            Console.WriteLine("Found user.");
            await EnterMainMenu(botClient, message);
        }
        else
        {
            Console.WriteLine("Registered new user.");
            var newUserInfo = new UserInfo();
            botStorage.IdToUserInfoDict.Add(message.From.Id, newUserInfo);
            
            await EnterMainMenu(botClient, message);
        }
    }

    private async Task EnterMainMenu(ITelegramBotClient botClient, Message message)
    {
        var userInfo = BotStorage.GetInstance().IdToUserInfoDict[message.From.Id];
        await botClient.SendTextMessageAsync(message.Chat.Id, "Меню:", replyMarkup: Keyboards.GetInstance().GetMainMenuKeyboard(userInfo));
        userInfo.UserState = UserState.Menu;
    }
}