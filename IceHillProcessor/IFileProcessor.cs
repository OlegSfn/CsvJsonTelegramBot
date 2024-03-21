namespace IceHillProcessor;

/// <summary>
/// Represents an interface for processing files related to ice hills.
/// </summary>
public interface IFileProcessor
{
    /// <summary>
    /// Writes ice hills data to a stream.
    /// </summary>
    /// <param name="iceHills">The array of ice hills data to write.</param>
    /// <returns>Returns a stream containing the written data.</returns>
    public Stream Write(IceHill[] iceHills);
    
    /// <summary>
    /// Reads ice hills data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the data to read.</param>
    /// <returns>Returns an array of ice hills data.</returns>
    public IceHill[] Read(Stream stream);
    
    /// <summary>
    /// Gets the file name associated with the processing.
    /// </summary>
    public string FileName { get; }
}