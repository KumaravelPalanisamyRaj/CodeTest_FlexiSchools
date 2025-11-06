namespace SchoolCanteensDomain;

// IdempotencyEntry.cs
public class IdempotencyEntry
{
    public string Key { get; set; } = null!; // Idempotency-Key header
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}