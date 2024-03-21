using Newtonsoft.Json;

namespace IceHillProcessor;

/// <summary>
/// Represents a class for processing JSON files related to ice hills.
/// </summary>
public class JSONProcessing : IFileProcessor
{
    private readonly string _userTelegramId;

    public JSONProcessing(string userTelegramId)
    {
        _userTelegramId = userTelegramId;
    }

    /// <summary>
    /// Writes ice hills data to a JSON file.
    /// </summary>
    /// <param name="iceHills">The array of ice hills data to write.</param>
    /// <returns>Returns a stream containing the written JSON data.</returns>
    public Stream Write(IceHill[] iceHills)
    {
        var serializer = new JsonSerializer();
        var filePath = Path.Combine("../../../../data", "temps", FileName);
        using (var sw = new StreamWriter(filePath))
        using (var writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, iceHills);
        }
        
        return File.OpenRead(filePath); 
    }

    /// <summary>
    /// Reads ice hills data from a JSON file.
    /// </summary>
    /// <param name="stream">The stream containing the JSON data to read.</param>
    /// <returns>Returns an array of ice hills data.</returns>
    public IceHill[] Read(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        var ser = new JsonSerializer();
        return ser.Deserialize<IceHill[]>(jsonReader);
    }
    
    /// <summary>
    /// Gets the file name associated with the current processing instance.
    /// </summary>
    public string FileName => $"temp{_userTelegramId}.json";
}