using Extensions;
using IceHillProcessor;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot;

public class Keyboards
{
    private static Keyboards s_instance;

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

    public ReplyKeyboardMarkup FileEditModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"Отфильтровать", "Отсортировать"},
        new KeyboardButton[] {"Удалить", "Скачать"}
    })
    {
        ResizeKeyboard = true
    };
    
    public ReplyKeyboardMarkup DownloadModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"Csv", "Json"},
    })
    {
        ResizeKeyboard = true
    };
    
    public ReplyKeyboardMarkup FilterFieldKeyboard => new (new[]
    {
        new KeyboardButton[] {"NameWinter", "HasEquipmentRental"},
        new KeyboardButton[] {"AdmArea", "HasWifi"},
        new KeyboardButton[] {"End"}
    })
    {
        ResizeKeyboard = true
    };
    
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
    
    public ReplyKeyboardMarkup SortFieldKeyboard => new (new[]
    {
        new KeyboardButton[] {"ServicesWinter", "UsagePeriodWinter"},
        new KeyboardButton[] {"End"}
    })
    {
        ResizeKeyboard = true
    };
    
    public ReplyKeyboardMarkup SortModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"По возрастанию", "По убыванию"}
    })
    {
        ResizeKeyboard = true
    };
    
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
    
    
    
    public static Keyboards GetInstance()
    {
        if (s_instance == null)
            s_instance = new Keyboards();
        
        return s_instance;
    } 
}

