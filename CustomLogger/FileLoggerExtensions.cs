using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Contains extension methods to add file logging to an ILoggerFactory.
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds a file logging provider to the given ILoggerFactory.
    /// </summary>
    /// <param name="factory">The ILoggerFactory to add the file logging provider to.</param>
    /// <param name="filePath">The path where log files will be stored.</param>
    /// <returns>Returns the provided ILoggerFactory with the file logging provider added.</returns>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));
        return factory;
    }
}