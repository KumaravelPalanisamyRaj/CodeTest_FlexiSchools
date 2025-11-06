using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolCanteensApplication;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _log;

    public OrdersController(IMediator mediator, ILogger<OrdersController> log) { _mediator = mediator; _log = log; }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand dto)
    {
        // Grab Idempotency-Key header if present
        if (Request.Headers.TryGetValue("Idempotency-Key", out var key))
            dto.IdempotencyKey = key.FirstOrDefault();
        try
        {
            var result = await _mediator.Send(dto);
            return CreatedAtAction(null, new { id = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            _log.LogInformation("Validation failed for order create: {Reasons}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}