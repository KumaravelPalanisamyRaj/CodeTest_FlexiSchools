namespace SchoolCanteensDomain;

// MenuItemStock.cs (per-day stock)
public class MenuItemStock
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public DateTime Date { get; set; } // date (date portion only)
    public int Remaining { get; set; }
    public byte[]? RowVersion { get; set; }
}