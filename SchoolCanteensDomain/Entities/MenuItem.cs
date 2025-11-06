namespace SchoolCanteensDomain;

// MenuItem.cs
public class MenuItem
{
    public Guid Id { get; set; }
    public Guid CanteenId { get; set; }
    public Canteen Canteen { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int? DailyStock { get; set; } // optional default daily total
    public List<string> Allergens { get; set; } = new();
}