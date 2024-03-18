using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Data;
using TelegramBot.Data.User;

namespace TelegramBot.Handlers;

public class UpdateHandler
{
    private readonly Dictionary<UserState, IAsyncHandler> _stateHandlers;
    private readonly Dictionary<string, IAsyncHandler> _commandsHandlers;
    private BotStorage _botStorage;
    private ILogger _logger;

    public UpdateHandler(BotStorage botStorage, ILogger logger)
    {
        _botStorage = botStorage;
        _logger = logger;

        var mainMenu = new MainMenu(botStorage, logger);
        _stateHandlers = new Dictionary<UserState, IAsyncHandler>
        {
            { UserState.None, new NoneStateHandler(botStorage, logger)},
            { UserState.Menu, new MenuStateHandler(botStorage, logger)},
            { UserState.EnteringNewFile, new NewFileStateHandler(botStorage, logger, mainMenu)},
            { UserState.EditingFile, new EditingFileStateHandler(botStorage, logger, mainMenu)},
            { UserState.ChoosingFile, new ChoosingFileStateHandler(botStorage, logger)},
            { UserState.FilteringFile, new FilteringFileStateHandler(botStorage, logger)},
            { UserState.SortingFile, new SortingFileStateHandler(botStorage, logger)},
            { UserState.DownloadingFile, new DownloadingFileStateHandler(botStorage, logger, mainMenu)},
            { UserState.ChoosingFilter, new ChoosingFilterStateHandler(botStorage, logger)},
            { UserState.ChoosingSortMode, new ChoosingSortModeStateHandler(botStorage, logger)},
            { UserState.SavingResults, new SavingResultsStateHandler(botStorage, logger, mainMenu)}
        };
        _commandsHandlers = new Dictionary<string, IAsyncHandler>
        {
            {"/start", new StartCommandHandler(botStorage, logger, mainMenu)}
        };
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.From == null)
            return;
        
        _logger.LogInformation($"{update.Message.From.Id} sent: {update.Message.Text ?? "(no text)"}.");

        // Handle commands.
        if (_commandsHandlers.TryGetValue(update.Message.Text ?? "", out var handler))
        {
            await handler.HandleAsync(botClient, update.Message);
            return;
        }
        
        // Handle messages.
        var user = _botStorage.IdToUserInfoDict[update.Message.From.Id];
        if (_stateHandlers.TryGetValue(user.UserState, out handler))
            await handler.HandleAsync(botClient, update.Message);
        else
            _logger.LogInformation($"{update.Message.From.Id} unhandled state - {user.UserState}");
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}