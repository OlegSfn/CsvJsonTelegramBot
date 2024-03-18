using CsvHelper.Configuration.Attributes;

namespace IceHillProcessor;

public class IceHill
{
    #region Fields
    private long _globalId;
    private string _objectName;
    private string _nameWinter;
    private string _photoWinter;
    private string _admArea;
    private string _district;
    private string _address;
    private string _email;
    private string _website;
    private string _helpPhone;
    private string _helpPhoneExtension;
    private string _workingHoursWinter;
    private string _clarificationOfWorkingHoursWinter;
    private string _hasEquipmentRental;
    private string _equipmentRentalComments;
    private string _hasTechService;
    private string _techServiceComments;
    private string _hasDressingRoom;
    private string _hasEatery;
    private string _hasToilet;
    private string _hasWifi;
    private string _hasCashMachine;
    private string _hasFirstAidPost;
    private string _hasMusic;
    private string _usagePeriodWinter;
    private string _dimensionsWinter;
    private string _lighting;
    private string _surfaceTypeWinter;
    private string _seats;
    private string _paid;
    private string _paidComments;
    private string _disabilityFriendly;
    private string _servicesWinter;
    private string _geoData;
    private string _geodata_center;
    private string _geoarea;
    #endregion

    #region Properties
    
    [Name("global_id")]
    public string GlobalId
    {
        get => _globalId.ToString();
        set => _globalId = long.Parse(value);
    }

    public string ObjectName
    {
        get => _objectName;
        set => _objectName = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string NameWinter
    {
        get => _nameWinter;
        set => _nameWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string PhotoWinter
    {
        get => _photoWinter;
        set => _photoWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string AdmArea
    {
        get => _admArea;
        set => _admArea = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string District
    {
        get => _district;
        set => _district = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Address
    {
        get => _address;
        set => _address = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Email
    {
        get => _email;
        set => _email = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Name("WebSite")]
    public string Website
    {
        get => _website;
        set => _website = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HelpPhone
    {
        get => _helpPhone;
        set => _helpPhone = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HelpPhoneExtension
    {
        get => _helpPhoneExtension;
        set => _helpPhoneExtension = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string WorkingHoursWinter
    {
        get => _workingHoursWinter;
        set => _workingHoursWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ClarificationOfWorkingHoursWinter
    {
        get => _clarificationOfWorkingHoursWinter;
        set => _clarificationOfWorkingHoursWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasEquipmentRental
    {
        get => _hasEquipmentRental;
        set => _hasEquipmentRental = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string EquipmentRentalComments
    {
        get => _equipmentRentalComments;
        set => _equipmentRentalComments = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasTechService
    {
        get => _hasTechService;
        set => _hasTechService = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string TechServiceComments
    {
        get => _techServiceComments;
        set => _techServiceComments = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasDressingRoom
    {
        get => _hasDressingRoom;
        set => _hasDressingRoom = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasEatery
    {
        get => _hasEatery;
        set => _hasEatery = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasToilet
    {
        get => _hasToilet;
        set => _hasToilet = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasWifi
    {
        get => _hasWifi;
        set => _hasWifi = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasCashMachine
    {
        get => _hasCashMachine;
        set => _hasCashMachine = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasFirstAidPost
    {
        get => _hasFirstAidPost;
        set => _hasFirstAidPost = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string HasMusic
    {
        get => _hasMusic;
        set => _hasMusic = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string UsagePeriodWinter
    {
        get => _usagePeriodWinter;
        set => _usagePeriodWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string DimensionsWinter
    {
        get => _dimensionsWinter;
        set => _dimensionsWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Lighting
    {
        get => _lighting;
        set => _lighting = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string SurfaceTypeWinter
    {
        get => _surfaceTypeWinter;
        set => _surfaceTypeWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Seats
    {
        get => _seats;
        set => _seats = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Paid
    {
        get => _paid;
        set => _paid = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string PaidComments
    {
        get => _paidComments;
        set => _paidComments = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string DisabilityFriendly
    {
        get => _disabilityFriendly;
        set => _disabilityFriendly = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ServicesWinter
    {
        get => _servicesWinter;
        set => _servicesWinter = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Name("geoData")]
    public string GeoData
    {
        get => _geoData;
        set => _geoData = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Name("geodata_center")]
    public string GeodataCenter
    {
        get => _geodata_center;
        set => _geodata_center = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Name("geoarea")]
    public string Geoarea
    {
        get => _geoarea;
        set => _geoarea = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion

    [Ignore]
    public string this[string field]
    => field switch
        {
            "NameWinter" => NameWinter,
            "HasEquipmentRental" => HasEquipmentRental,
            "AdmArea" => AdmArea,
            "HasWifi" => HasWifi,
            "ServicesWinter" => ServicesWinter,
            "UsagePeriodWinter" => UsagePeriodWinter,
            _ => throw new ArgumentException("There is no such field or this field isn't supported yet.")
        };

    public DateTime OpenDate
    {
        get
        {
            var startDate = UsagePeriodWinter.Split('-')[0].Split('.');
            (int day, int month) date = (int.Parse(startDate[0]), int.Parse(startDate[1]));
            return new DateTime(DateTime.Now.Year, date.month, date.day);
        }
    } 
        
    
}