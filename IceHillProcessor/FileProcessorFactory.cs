namespace IceHillProcessor;

/// <summary>
/// Represents a factory for creating file processors based on file extensions.
/// </summary>
public class FileProcessorFactory
{
    private readonly string _userTelegramId;
    private string _fileName;
    
    public FileProcessorFactory(string userTelegramId, string fileName)
    {
        _userTelegramId = userTelegramId;
        _fileName = fileName;
    }

    public FileProcessorFactory() { }

    /// <summary>
    /// Creates an instance of the appropriate file processor based on the file extension.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IFileProcessor"/> corresponding to the file extension.</returns>
    public IFileProcessor CreateFileProcessor()
    {
        if (_fileName.EndsWith(".csv"))
            return new CSVProcessing(_userTelegramId);
        if (_fileName.EndsWith(".json"))
            return new JSONProcessing(_userTelegramId);

        throw new ArgumentException($"{nameof(_fileName)} has not supported file format.");
    }
}