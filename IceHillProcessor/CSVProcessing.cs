using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace IceHillProcessor;
public class CSVProcessing : IFileProcessor
{
    public Stream Write(IceHill[] iceHills)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            ShouldQuote = _ => true
        };
        
        using (var writer = new StreamWriter(Path.Combine("../../../../", "data", "temp.csv")))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteHeader<IceHill>();
            csv.NextRecord();
            foreach (var record in iceHills)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
        
        return File.OpenRead(Path.Combine("../../../../", "data", "temp.csv")); 
    }

    public IceHill[] Read(Stream stream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            PrepareHeaderForMatch = arg => arg.Header.Trim('"')
        };
        
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Read();
            csv.ReadHeader();

            #region validate second row
            csv.Read();
            var secondRow = csv.Parser.Record;
            if (secondRow == null)
                throw new ArgumentException("Bad data. There is no second header.");
            if (secondRow.Length != csv.HeaderRecord.Length)
                throw new ArgumentException("Bad data. Second header must have the same length as the first.");
            #endregion
            
            return csv.GetRecords<IceHill>().ToArray();
        }
    }
    
    
}