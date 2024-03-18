using Newtonsoft.Json;

namespace IceHillProcessor;

public class JSONProcessing : IFileProcessor
{
    public Stream Write(IceHill[] iceHills)
    {
        var serializer = new JsonSerializer();
        using (StreamWriter sw = new StreamWriter(Path.Combine("../../../../", "data", "temp.json")))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, iceHills);
        }
        
        return File.OpenRead(Path.Combine("../../../../", "data", "temp.json")); 
    }

    public IceHill[] Read(Stream stream)
    {
        using (StreamReader reader = new StreamReader(stream))
        using (JsonTextReader jsonReader = new JsonTextReader(reader))
        {
            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<IceHill[]>(jsonReader);
        }
    }
}