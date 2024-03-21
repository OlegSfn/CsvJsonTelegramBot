using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Represents a provider for creating instances of the FileLogger class.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    public FileLoggerProvider(string path)
    {
        _path = path;
    }

    /// <summary>
    /// Creates a new instance of the FileLogger class.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>Returns a new instance of the FileLogger class.</returns>
    public ILogger CreateLogger(string categoryName)
        => new FileLogger(_path);
    
    /// <summary>
    /// Disposes of any resources used by the FileLoggerProvider.
    /// </summary>
    public void Dispose() { }
}