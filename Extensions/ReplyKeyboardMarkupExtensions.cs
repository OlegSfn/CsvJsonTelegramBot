using Telegram.Bot.Types.ReplyMarkups;

namespace Extensions;

public static class ReplyKeyboardMarkupExtensions
{
    public static HashSet<string> KeyboardToVariants(this ReplyKeyboardMarkup keyboard)
    {
        var results = new HashSet<string>();
        foreach (var row in keyboard.Keyboard)
            foreach (var button in row)
                results.Add(button.Text);
        
        return results;
    }
}