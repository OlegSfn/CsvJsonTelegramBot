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
                    new KeyboardButton[] { "Enter new file", "Edit file", "Download file" }
                }) 
            {
                ResizeKeyboard = true
            };
        
        return new ReplyKeyboardMarkup(new[]
        { 
            new KeyboardButton[] { "Enter new file"}
        }) 
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup FileEditModeKeyboard => new (new[]
    {
        new KeyboardButton[] {"Filter", "Sort"},
        new KeyboardButton[] {"Delete"}
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
        new KeyboardButton[] {"Ascending", "Descending"}
    })
    {
        ResizeKeyboard = true
    };
    
    public ReplyKeyboardMarkup GetFileNamesKeyboard(UserInfo userInfo, string userID)
    {
        var fileNamesButtons = new List<IEnumerable<KeyboardButton>>();
        for (int i = 0; i < userInfo.FileNames.Count/4; i++)
            fileNamesButtons.Add(userInfo.FileNames.Skip(4 * i).Take(4).Select(x => new KeyboardButton(PathExtensions.DBToUserFileName(x, userID))));
        fileNamesButtons.Add(userInfo.FileNames.TakeLast(userInfo.FileNames.Count%4).Select(x => new KeyboardButton(PathExtensions.DBToUserFileName(x, userID))));

        
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

