namespace IceHillProcessor;

public class FileProcessorFactory
{
    private string _userTelegramId;
    private string _fileName;
    
    public FileProcessorFactory(string userTelegramId, string fileName)
    {
        _userTelegramId = userTelegramId;
        _fileName = fileName;
    }

    public IFileProcessor CreateFileProcessor()
    {
        if (_fileName.EndsWith(".csv"))
            return new CSVProcessing(_userTelegramId);
        if (_fileName.EndsWith(".json"))
            return new JSONProcessing(_userTelegramId);

        throw new ArgumentException($"{nameof(_fileName)} has not supported file format.");
    }
}