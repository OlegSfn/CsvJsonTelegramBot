namespace IceHillProcessor;

public class FileProcessorFactory
{
    private string _fileName;
    
    public FileProcessorFactory(string fileName)
    {
        _fileName = fileName;
    }

    public IFileProcessor CreateFileProcessor()
    {
        if (_fileName.EndsWith(".csv"))
            return new CSVProcessing();
        if (_fileName.EndsWith(".json"))
            return new JSONProcessing();

        throw new ArgumentException($"{nameof(_fileName)} has not supported file format.");
    }
}