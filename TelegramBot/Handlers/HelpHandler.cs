using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers;

public class HelpHandler : IAsyncHandler
{
    private readonly ILogger _logger;
    private readonly MainMenu _mainMenu;
    
    public HelpHandler(ILogger logger, MainMenu mainMenu)
    {
        _logger = logger;
        _mainMenu = mainMenu;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message)
    {
        _logger.LogInformation($"Sent help to {message.From.Id}.");
        var helpText = "Этот бот предназначен для обработки файлов в форматах CSV и JSON, содержащих информацию о зимних спортивных объектах. Он может фильтровать данные по следующим параметрам:\n\n" +
                           "1) NameWinter: Название спортивной зоны в зимний период.\n" +
                           "2) HasEquipmentRental: Наличие возможности проката оборудования.\n" + 
                           "3) AdmArea: Административный округ, в котором расположен спортивный объект.\n" +
                           "4) HasWifi: Наличие точки Wi-Fi.\n" +
                           "5) ServicesWinter: Услуги, предоставляемые в зимний период.\n\n" +
                       "Также бот может сортировать данные по следующим параметрам:\n" + 
                           "1) ServicesWinter: Услуги, предоставляемые в зимний период.\n" +
                           "2) UsagePeriodWinter: Период эксплуатации в зимний период.\n";
        
        await botClient.SendTextMessageAsync(message.From.Id, helpText);
        await _mainMenu.EnterMainMenuAsync(botClient, message);
    }
}