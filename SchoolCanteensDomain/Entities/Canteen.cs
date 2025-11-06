namespace SchoolCanteensDomain;
// Canteen.cs
public class Canteen
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    // Which weekdays the canteen is open
    public List<DayOfWeek> OpeningDays { get; set; } = new();
    // Daily cut-off time (local time-of-day)
    public TimeSpan CutoffTime { get; set; }
}