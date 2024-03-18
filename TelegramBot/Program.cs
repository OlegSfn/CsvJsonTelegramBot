using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using CustomLogger;
using TelegramBot.Data;

namespace TelegramBot;
// /Users/oleg_sfn/Downloads/ice-hills.csv
//TODO: If the data directory is not in place then DirectoryNotFound exc is released. 
//TODO: Check when int values are not int in file.
//TODO: Check that data is correct.
public class Program
{
    public static async Task Main()
    {
        #region TestCsv
        // var csvProcessor = new CSVProcessing();
        // IceHill[] iceHills = csvProcessor.Read(System.IO.File.OpenRead("/Users/oleg_sfn/Downloads/ice-hills.csv"));
        // Console.WriteLine(iceHills);
        #endregion
        
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
        BotStorage.GetInstance().Logger = logger;
        
        
        var botClient = new TelegramBotClient("1803799892:AAHr34l5WZnpJkovnE75C_kYMmDlrmOqMgc");
        
        using CancellationTokenSource cts = new ();
        
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };
        
        var updateHandler = new Handlers.UpdateHandler();
        botClient.StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        
        Console.ReadLine(); // Needed to not end the program immediately.
        cts.Cancel();
    }
    
    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}


