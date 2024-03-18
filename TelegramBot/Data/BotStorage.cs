using Microsoft.Extensions.Logging;
using TelegramBot.Data.User;

namespace TelegramBot.Data;

public class BotStorage
{
    private static BotStorage s_instance;
    public Dictionary<long, UserInfo> IdToUserInfoDict = new();
    
    public static BotStorage GetInstance()
    {
        if (s_instance == null)
            s_instance = new BotStorage();
        
        return s_instance;
    } 
}