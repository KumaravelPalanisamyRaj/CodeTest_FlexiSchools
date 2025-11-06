using MediatR;

namespace SchoolCanteensApplication;
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public string? IdempotencyKey { get; set; }
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CanteenId { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
public class CreateOrderItemDto
{ 
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; } 
}
public class CreateOrderResult
{ 
    public Guid OrderId { get; set; }
    public decimal Total { get; set; } 
}
