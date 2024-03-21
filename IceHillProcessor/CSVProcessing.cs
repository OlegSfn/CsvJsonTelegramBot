using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace IceHillProcessor;

/// <summary>
/// Represents a class for processing CSV files related to ice hills.
/// </summary>
public class CSVProcessing : IFileProcessor
{
    private readonly string _userTelegramId;
    
    private static readonly string[] s_headersRus =
    {
        "global_id", "Название спортивного объекта", "Название спортивной зоны в зимний период",
        "Фотография в зимний период", "Административный округ", "Район", "Адрес", "Адрес электронной почты",
        "Адрес сайта", "Справочный телефон", "Добавочный номер", "График работы в зимний период",
        "Уточнение графика работы в зимний период", "Возможность проката оборудования",
        "Комментарии для проката оборудования", "Наличие сервиса технического обслуживания",
        "Комментарии для сервиса технического обслуживания", "Наличие раздевалки", "Наличие точки питания",
        "Наличие туалета", "Наличие точки Wi-Fi", "Наличие банкомата", "Наличие медпункта",
        "Наличие звукового сопровождения", "Период эксплуатации в зимний период", "Размеры в зимний период",
        "Освещение", "Покрытие в зимний период", "Количество оборудованных посадочных мест",
        "Форма посещения (платность)", "Комментарии к стоимости посещения", "Приспособленность для занятий инвалидов",
        "Услуги предоставляемые в зимний период", "geoData", "geodata_center", "geoarea"
    };

    public CSVProcessing(string userTelegramId)
    {
        _userTelegramId = userTelegramId;
    }

    public CSVProcessing() { }

    /// <summary>
    /// Writes ice hills data to a CSV file.
    /// </summary>
    /// <param name="iceHills">The array of ice hills data to write.</param>
    /// <returns>Returns a stream containing the written CSV data.</returns>
    public Stream Write(IceHill[] iceHills)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            ShouldQuote = _ => true
        };

        var filePath = Path.Combine("../../../../data", "temps", FileName);
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteHeader<IceHill>();
            csv.NextRecord();
            writer.WriteLine(string.Join(',', s_headersRus.Select(x => $"\"{x}\"")));
            foreach (var record in iceHills)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
        
        return File.OpenRead(filePath); 
    }

    /// <summary>
    /// Reads ice hills data from a CSV file.
    /// </summary>
    /// <param name="stream">The stream containing the CSV data to read.</param>
    /// <returns>Returns an array of ice hills data.</returns>
    public IceHill[] Read(Stream stream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            PrepareHeaderForMatch = arg => arg.Header.Trim().Trim('"')
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);
        
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

    /// <summary>
    /// Gets the file name associated with the current processing instance.
    /// </summary>
    public string FileName => $"temp{_userTelegramId}.csv";
}