namespace IceHillProcessor;

public interface IFileProcessor
{
    public Stream Write(IceHill[] iceHills);
    public IceHill[] Read(Stream stream);
}