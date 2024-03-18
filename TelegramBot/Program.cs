using System.Xml.Schema;
using Telegram.Bot;
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
//TODO: Add more info to logging.
public class Program
{
    public static async Task Main()
    {
        #region TestCsv
        // var csvProcessor = new CSVProcessing();
        // IceHill[] iceHills = csvProcessor.Read(System.IO.File.OpenRead("/Users/oleg_sfn/Desktop/ice-hills-big.csv"));
        // Console.WriteLine(iceHills);
        // csvProcessor.Write(iceHills);
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


