using Newtonsoft.Json;

namespace IceHillProcessor;

public class JSONProcessing : IFileProcessor
{
    private readonly string _userTelegramId;

    public JSONProcessing(string userTelegramId)
    {
        _userTelegramId = userTelegramId;
    }

    public Stream Write(IceHill[] iceHills)
    {
        var serializer = new JsonSerializer();
        var filePath = Path.Combine("../../../../data", "temps", $"temp{_userTelegramId}.json");
        using (var sw = new StreamWriter(filePath))
        using (var writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, iceHills);
        }
        
        return File.OpenRead(filePath); 
    }

    public IceHill[] Read(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        var ser = new JsonSerializer();
        return ser.Deserialize<IceHill[]>(jsonReader);
    }
}