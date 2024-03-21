using CustomLogger;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramBot.Data;
using TelegramBot.Handlers;

namespace TelegramBot;

/// <summary>
/// Represents the Telegram bot application.
/// </summary>
public class Bot
{
    private const string Token = "7111138965:AAEIbvZ5b4Latoj1pzVUgBCLI2ThAJdDYZM";
    private readonly ReceiverOptions _receiverOptions;
    private readonly CancellationTokenSource _cts;
    private readonly BotStorage _storage;
    private readonly ILogger _logger;
    
    public Bot(ReceiverOptions receiverOptions, CancellationTokenSource cts)
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

        _logger = logger;
        _storage = new BotStorage();
        _receiverOptions = receiverOptions;
        _cts = cts;
    }

    /// <summary>
    /// Starts the Telegram bot.
    /// </summary>
    public void Start()
    {
        var updateHandler = new UpdateHandler(_storage, _logger);
        var errorHandler = new ErrorHandler(_logger);
        new TelegramBotClient(Token).StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            pollingErrorHandler: errorHandler.HandleErrorAsync,
            receiverOptions: _receiverOptions,
            cancellationToken: _cts.Token
        );
        
        _logger.LogInformation("Bot started.");
    }
}