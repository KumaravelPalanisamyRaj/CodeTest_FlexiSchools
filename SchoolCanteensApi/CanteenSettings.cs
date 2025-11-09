using Microsoft.Extensions.Configuration;

public class CanteenSettings
{
    public int MaxOrderQuantity { get; set; } = 10;
    public string CanteenName { get; set; } = string.Empty;
    public string DefaultDataConnection { get; set; } = string.Empty;
    public bool UseInMemoryData { get; set; } = true;
}

public sealed class CanteenConfig
{
    private static readonly object _lock = new object();
    private static CanteenConfig? _instance;

    private readonly CanteenSettings _settings;

    private CanteenConfig()
    {
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Bind "CanteenSettings" section to CanteenSettings object
        _settings = configuration.GetSection("CanteenSettings").Get<CanteenSettings>()
                    ?? new CanteenSettings(); // fallback to defaults if section is missing
    }

    public static CanteenConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CanteenConfig();
                    }
                }
            }
            return _instance;
        }
    }

    // Expose settings as read-only properties
    public int MaxOrderQuantity => _settings.MaxOrderQuantity;
    public string CanteenName => _settings.CanteenName;
    public string DefaultDataConnection => _settings.DefaultDataConnection;
    public bool UseInMemoryData => _settings.UseInMemoryData;

    // Optional: expose the full settings object
    public CanteenSettings Current => _settings;
}
