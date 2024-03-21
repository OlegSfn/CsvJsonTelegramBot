using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Represents a file logger implementation for Microsoft.Extensions.Logging.
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _filePath;
    private static readonly object s_Lock = new();
    public FileLogger(string path)
    {
        _filePath = path;
    }
    
    /// <summary>
    /// This method does not provide any scope context.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin the scope with.</typeparam>
    /// <param name="state">The state to begin the scope with.</param>
    /// <returns>Returns null as this logger does not support scope context.</returns>
    public IDisposable? BeginScope<TState>(TState state)
    {
        return null;
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>Returns true as logging is always enabled.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <summary>
    /// Writes a log message to the file.
    /// </summary>
    /// <typeparam name="TState">The type of the state to be logged.</typeparam>
    /// <param name="logLevel">The log level of the log message.</param>
    /// <param name="eventId">The event ID of the log message.</param>
    /// <param name="state">The state to be logged.</param>
    /// <param name="exception">The exception related to the log message (if any).</param>
    /// <param name="formatter">A delegate function to format the log message.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string>? formatter)
    {
        if (formatter == null) return;

        var exc = "";
        lock (s_Lock)
        {
            var fullFilePath = Path.Combine(_filePath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            var n = Environment.NewLine;
            if (exception != null)
                exc = n + exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;
            File.AppendAllText(fullFilePath, $"{DateTime.Now:T} {logLevel}: {formatter(state, exception)}{n}{exc}");
        }
    }
}