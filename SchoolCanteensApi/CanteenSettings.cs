
using Microsoft.Extensions.Options;

public class CanteenSettings
{
    public int MaxOrderQuantity { get; set; } = 10;
    public string CanteenName { get; set; } = string.Empty;
    public string DefaultDataConnection { get; set; } = string.Empty;
    public bool UseInMemoryData { get; set; } = true;
}


public static class CanteenConfig
{
    private static IOptionsMonitor<CanteenSettings>? _optionsMonitor;

    // Initialize once in Program.cs
    public static void Initialize(IOptionsMonitor<CanteenSettings> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    // Simple getter
    public static CanteenSettings Current => _optionsMonitor!.CurrentValue;
}
