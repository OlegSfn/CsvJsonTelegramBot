using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

/// <summary>
/// Represents a handler for providing help information to users.
/// </summary>
public class HelpHandler : IAsyncHandler
{
    private readonly ILogger _logger;
    private readonly TransitionToMenuHandler _transitionToMenuHandler;
    
    public HelpHandler(ILogger logger, TransitionToMenuHandler transitionToMenuHandler)
    {
        _logger = logger;
        _transitionToMenuHandler = transitionToMenuHandler;
    }
    
    public HelpHandler() { }

    /// <summary>
    /// Handles the help request by sending help information to the user.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="message">The message sent by the user.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"Sent help to {message.From.Id}.");
        const string helpText = "Этот бот предназначен для обработки файлов в форматах CSV и JSON, содержащих информацию о зимних спортивных объектах. Он может фильтровать данные по следующим параметрам:\n\n" +
                                "1) NameWinter: Название спортивной зоны в зимний период.\n" +
                                "2) HasEquipmentRental: Наличие возможности проката оборудования.\n" + 
                                "3) AdmArea: Административный округ, в котором расположен спортивный объект.\n" +
                                "4) HasWifi: Наличие точки Wi-Fi.\n" +
                                "5) ServicesWinter: Услуги, предоставляемые в зимний период.\n\n" +
                                "Также бот может сортировать данные по следующим параметрам:\n" + 
                                "1) ServicesWinter: Услуги, предоставляемые в зимний период.\n" +
                                "2) UsagePeriodWinter: Период эксплуатации в зимний период.\n\n" +
                                "Бот поддерживает следующие команды:\n" +
                                "1) /start - Переход в меню, запуск бота.\n" +
                                "2) /help - Получить помощь.\n" +
                                "3) /examples - Получить примеры файлов.";
        
        await botClient.SendTextMessageAsync(message.From.Id, helpText);
        await _transitionToMenuHandler.HandleAsync(botClient, message);
    }
}