using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Data;
using TelegramBot.Data.User;
using TelegramBot.Handlers.FileEditing;
using TelegramBot.Handlers.FileIO;

namespace TelegramBot.Handlers;

/// <summary>
/// Handles updates received from the Telegram bot.
/// </summary>
public class UpdateHandler
{
    private readonly Dictionary<UserState, IAsyncHandler> _stateHandlers;
    private readonly Dictionary<string, IAsyncHandler> _commandsHandlers;
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly BotStorage _botStorage;
    private readonly ILogger _logger;

    public UpdateHandler(BotStorage botStorage, ILogger logger)
    {
        var mainMenu = new TransitionToMenuHandler(botStorage, logger);
        var helpHandler = new HelpHandler(logger, mainMenu);
        _stateHandlers = new Dictionary<UserState, IAsyncHandler>
        {
            { UserState.None, new NoneStateHandler(logger) },
            { UserState.Menu, new MenuStateHandler(botStorage, logger, helpHandler) },
            { UserState.EnteringNewFile, new NewFileStateHandler(botStorage, logger, mainMenu) },
            { UserState.EditingFile, new EditingFileStateHandler(botStorage, logger, mainMenu) },
            { UserState.ChoosingFile, new ChoosingFileStateHandler(botStorage, logger) },
            { UserState.FilteringFile, new FilteringFileStateHandler(botStorage, logger) },
            { UserState.SortingFile, new SortingFileStateHandler(botStorage, logger) },
            { UserState.DownloadingFile, new DownloadingFileStateHandler(botStorage, logger, mainMenu) },
            { UserState.ChoosingFilter, new ChoosingFilterStateHandler(botStorage, logger) },
            { UserState.ChoosingSortMode, new ChoosingSortModeStateHandler(botStorage, logger) },
            { UserState.SavingResults, new SavingResultsStateHandler(botStorage, logger, mainMenu) }
        };
        _commandsHandlers = new Dictionary<string, IAsyncHandler>
        {
            { "/start", mainMenu },
            { "/help", helpHandler },
            { "/examples", new AskExamplesHandler(logger, mainMenu) }
        };

        _registerUserHandler = new RegisterUserHandler(botStorage, logger);
        _botStorage = botStorage;
        _logger = logger;
    }
    
    public UpdateHandler() { }
    
    /// <summary>
    /// Handles the incoming update from the Telegram bot.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="update">The update received from the Telegram bot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.From == null)
            return;
        
        _logger.LogInformation($"{update.Message.From.Id} sent: \"{update.Message.Text ?? "(no text)"}\".");

        try
        {
            // Register user if he is not registered.
            if (!_botStorage.IdToUserInfoDict.TryGetValue(update.Message.From.Id, out var user))
            {
                await _registerUserHandler.HandleAsync(botClient, update.Message);
                user = _botStorage.IdToUserInfoDict[update.Message.From.Id];
            }

            // Handle commands.
            if (_commandsHandlers.TryGetValue(update.Message.Text ?? "", out var handler))
            {
                await handler.HandleAsync(botClient, update.Message);
                return;
            }

            // Handle messages.
            if (_stateHandlers.TryGetValue(user.UserState, out handler))
                await handler.HandleAsync(botClient, update.Message);
            else
                _logger.LogInformation($"{update.Message.From.Id} unhandled state - {user.UserState}");
        }
        catch (Exception e)
        {
            await botClient.SendTextMessageAsync(update.Message.From.Id,
                "Упс, кажется, вы обидели бота :(\nНажмите /start, чтобы продолжить работу.", 
                cancellationToken: cancellationToken);

            _botStorage.IdToUserInfoDict.Remove(update.Message.From.Id);
            _logger.LogCritical($"{update.Message.From.Id} crashed bot with: {e.Message}");
        }
    }
}