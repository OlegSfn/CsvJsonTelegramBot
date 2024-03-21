using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using CustomLogger;
using TelegramBot.Data;

namespace TelegramBot;
public class Program
{
    public static Task Main()
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
        loggerFactory.AddFile("../../../../var");
        var logger = loggerFactory.CreateLogger<Program>();
        
        var botClient = new TelegramBotClient("7111138965:AAEIbvZ5b4Latoj1pzVUgBCLI2ThAJdDYZM");

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
        
        Thread.Sleep(-1);
        cts.Cancel();
        return Task.CompletedTask;
    }
}


