using Extensions;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data.User;

namespace TelegramBot;

/// <summary>
/// Provides methods to generate various reply keyboards for the Telegram bot.
/// </summary>
public class Keyboards
{
    private static Keyboards s_instance;

    public static Keyboards GetInstance()
    {
        if (s_instance == null)
            s_instance = new Keyboards();
        
        return s_instance;
    } 
    
    /// <summary>
    /// Gets the main menu keyboard.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <returns>The main menu keyboard.</returns>
    public ReplyKeyboardMarkup GetMainMenuKeyboard(UserInfo userInfo)
    {
        if (userInfo.FileNames.Count > 0)
            return new ReplyKeyboardMarkup(new[]
                { 
                    new KeyboardButton[] { "Добавить файл", "Выбрать файл"},
                    new KeyboardButton[] { "Помощь"}
                }) 
            {
                ResizeKeyboard = true
            };
        
        return new ReplyKeyboardMarkup(new[]
        { 
            new KeyboardButton[] { "Добавить файл"},
            new KeyboardButton[] { "Помощь"}
        }) 
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Gets the file edit mode keyboard.
    /// </summary>
    public ReplyKeyboardMarkup FileEditModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"Отфильтровать", "Отсортировать"},
        new KeyboardButton[] {"Удалить", "Скачать"}
    })
    {
        ResizeKeyboard = true
    };
    
    /// <summary>
    /// Gets the download mode keyboard.
    /// </summary>
    public ReplyKeyboardMarkup DownloadModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"Csv", "Json"},
    })
    {
        ResizeKeyboard = true
    };
    
    /// <summary>
    /// Gets the filter field keyboard.
    /// </summary>
    public ReplyKeyboardMarkup FilterFieldKeyboard => new (new[]
    {
        new KeyboardButton[] {"NameWinter", "HasEquipmentRental"},
        new KeyboardButton[] {"AdmArea", "HasWifi"},
        new KeyboardButton[] {"End"}
    })
    {
        ResizeKeyboard = true
    };
    
    /// <summary>
    /// Gets the filter value keyboard.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <returns>The filter value keyboard.</returns>
    public ReplyKeyboardMarkup GetFilterValueKeyboard(UserInfo userInfo)
    {
        var uniqueFilterValues = new HashSet<string>();
        foreach (var iceHill in userInfo.CurIceHills)
            uniqueFilterValues.Add(iceHill[userInfo.FieldToEdit]);

        var fileNamesButtons = new List<IEnumerable<KeyboardButton>>();
        for (int i = 0; i < uniqueFilterValues.Count/4; i++)
            fileNamesButtons.Add(uniqueFilterValues.Skip(4 * i).Take(4).Select(x => new KeyboardButton(x)));
        fileNamesButtons.Add(uniqueFilterValues.TakeLast(uniqueFilterValues.Count%4).Select(x => new KeyboardButton(x)));

        
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(fileNamesButtons)
        {
            ResizeKeyboard = true
        };

        return replyKeyboardMarkup;
    }
    
    /// <summary>
    /// Gets the sort field keyboard.
    /// </summary>
    public ReplyKeyboardMarkup SortFieldKeyboard => new (new[]
    {
        new KeyboardButton[] {"ServicesWinter", "UsagePeriodWinter"},
        new KeyboardButton[] {"End"}
    })
    {
        ResizeKeyboard = true
    };
    
    /// <summary>
    /// Gets the sort mode keyboard.
    /// </summary>
    public ReplyKeyboardMarkup SortModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"По возрастанию", "По убыванию"}
    })
    {
        ResizeKeyboard = true
    };
    
    /// <summary>
    /// Gets the file names keyboard.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The file names keyboard.</returns>
    public ReplyKeyboardMarkup GetFileNamesKeyboard(UserInfo userInfo, string userId)
    {
        var fileNamesButtons = new List<IEnumerable<KeyboardButton>>();
        for (int i = 0; i < userInfo.FileNames.Count/4; i++)
            fileNamesButtons.Add(userInfo.FileNames.Skip(4 * i).Take(4).Select(x => new KeyboardButton(x.DBToUserFileName(userId))));
        fileNamesButtons.Add(userInfo.FileNames.TakeLast(userInfo.FileNames.Count%4).Select(x => new KeyboardButton(x.DBToUserFileName(userId))));

        
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(fileNamesButtons)
        {
            ResizeKeyboard = true
        };

        return replyKeyboardMarkup;
    }
}

