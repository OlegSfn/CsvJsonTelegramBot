using System.Xml.Schema;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using CustomLogger;
using TelegramBot.Data;

namespace TelegramBot;
public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        });
        loggerFactory.AddFile(Path.Combine("../../../../", "var"));
        var logger = loggerFactory.CreateLogger<Program>();
        
        var botClient = new TelegramBotClient("1803799892:AAHr34l5WZnpJkovnE75C_kYMmDlrmOqMgc");

        using CancellationTokenSource cts = new ();
        
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };
        
        var updateHandler = new Handlers.UpdateHandler(BotStorage.GetInstance(), logger);
        botClient.StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            pollingErrorHandler: updateHandler.HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        
        Console.ReadLine(); // Needed to not end the program immediately.
        cts.Cancel();
    }
}


