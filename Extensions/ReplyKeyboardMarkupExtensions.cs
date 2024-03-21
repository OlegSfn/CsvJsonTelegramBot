using Telegram.Bot.Types.ReplyMarkups;

namespace Extensions;

/// <summary>
/// Contains extension methods for working with Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup.
/// </summary>
public static class ReplyKeyboardMarkupExtensions
{
    /// <summary>
    /// Converts the buttons of a ReplyKeyboardMarkup to a HashSet of button texts.
    /// </summary>
    /// <param name="keyboard">The ReplyKeyboardMarkup instance to extract button texts from.</param>
    /// <returns>Returns a HashSet containing the text of each button in the ReplyKeyboardMarkup.</returns>
    public static HashSet<string> KeyboardToVariants(this ReplyKeyboardMarkup keyboard)
    {
        var results = new HashSet<string>();
        foreach (var row in keyboard.Keyboard)
            foreach (var button in row)
                results.Add(button.Text);
        
        return results;
    }
}