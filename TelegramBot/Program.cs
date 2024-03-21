using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot;
public class Program
{
    public static Task Main()
    {
        using CancellationTokenSource cts = new ();
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };

        var botClient = new Bot(receiverOptions, cts);
        botClient.Start();
        
        // Needed to work properly on server with nohup.
        Thread.Sleep(-1);
        cts.Cancel();
        return Task.CompletedTask;
    }
}


