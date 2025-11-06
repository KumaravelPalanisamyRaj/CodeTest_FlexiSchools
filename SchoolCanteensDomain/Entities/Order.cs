namespace SchoolCanteensDomain;

// Order.cs
public enum OrderState { Placed, Fulfilled, Cancelled }
public class Order
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = null!;
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public Guid CanteenId { get; set; }
    public Canteen Canteen { get; set; } = null!;
    public DateTime FulfilmentDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderState State { get; set; }
    public DateTime CreatedAt { get; set; }
}