
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolCanteensPersistence;
using SchoolCanteensDomain;
using SchoolCanteensInfrastructure;
using Microsoft.Extensions.Logging;

namespace SchoolCanteensApplication;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly SchoolCanteensDbContext _db;
    private readonly IIdempotencyService _idem;
    //private readonly ILogger<CreateOrderHandler> _log;
    public CreateOrderHandler(SchoolCanteensDbContext db, IIdempotencyService idem, ILogger<CreateOrderHandler> log)
    {
        _db = db; _idem = idem; //_log = log;
    }
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Idempotency: if key present, check store
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _idem.GetAsync(request.IdempotencyKey);
            if (existing?.OrderId != null)
            {
                //_log.LogInformation("Idempotent request - returning existing order {OrderId}", existing.OrderId);
                var ord = await _db.Orders.FindAsync(new object[] { existing.OrderId.Value }, cancellationToken);
                return new CreateOrderResult { OrderId = ord!.Id, Total = ord.Total };
            }
        }
        // Load required entities
        var parent = await _db.Parents.FindAsync(new object[] { request.ParentId }, cancellationToken) ?? throw new Exception("Parent not found");
        var student = await _db.Students.FindAsync(new object[] { request.StudentId }, cancellationToken) ?? throw new Exception("Student not found");
        var canteen = await _db.Canteens.FindAsync(new object[] { request.CanteenId }, cancellationToken) ?? throw new Exception("Canteen not found");
        // Validate student-parent link
        if (student.ParentId != parent.Id) 
            throw new Exception("Student does not belong to parent");
        // Cutoff check
        var today = request.FulfilmentDate.Date;
        var now = DateTime.Now; // server local time - in real app consider timezone per school
        if (!canteen.OpeningDays.Any(d => d == request.FulfilmentDate.DayOfWeek))
            throw new Exception("Canteen is closed on the requested fulfilment date");
        if (now.TimeOfDay > canteen.CutoffTime && request.FulfilmentDate.Date == now.Date)
            throw new Exception($"Order after cut-off time ({canteen.CutoffTime}).");
        // Build order items and compute total
        var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuItems = await _db.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToListAsync(cancellationToken);
        var itemsWithQty = request.Items.Select(i => (MenuItem: menuItems.Single(mi => mi.Id == i.MenuItemId), Quantity: i.Quantity)).ToList();
        // Allergens
        var studentAllergens = student.Allergens ?? new List<string>();
        var offending = itemsWithQty.Where(i => i.MenuItem.Allergens != null && i.MenuItem.Allergens.Intersect(studentAllergens).Any()).ToList();
        if (offending.Any()) 
            throw new Exception("Order contains items with student's allergens: " + string.Join(", ", offending.Select(x =>
        x.MenuItem.Name)));
        // Stock checks and reservations: read MenuItemStock for date (or create based on MenuItem.DailyStock)
        foreach (var entry in itemsWithQty)
        {
            var stock = await _db.MenuItemStocks.FirstOrDefaultAsync(s => s.MenuItemId == entry.MenuItem.Id && s.Date == today, cancellationToken);
            if (stock == null)
            {
                var initial = entry.MenuItem.DailyStock ?? int.MaxValue; // unlimited if null
                stock = new MenuItemStock { Id = Guid.NewGuid(), MenuItemId = entry.MenuItem.Id, Date = today, Remaining = initial };
                _db.MenuItemStocks.Add(stock);
                await _db.SaveChangesAsync(cancellationToken); // ensure row exists for concurrency token
            }
            if (entry.Quantity > stock.Remaining) 
                throw new Exception($"Not enough stock for {entry.MenuItem.Name}. Requested {entry.Quantity}, remaining {stock.Remaining}");
        }
        // Total
        decimal total = itemsWithQty.Sum(i => i.MenuItem.Price * i.Quantity);
        if (parent.WalletBalance < total) 
            throw new Exception($"Insufficient wallet balance. Required {total}, available {parent.WalletBalance}");
        // All checks passed -> create order within transaction and update stock and wallet
        using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                ParentId = parent.Id,
                StudentId = student.Id,
                CanteenId = canteen.Id,
                FulfilmentDate = request.FulfilmentDate.Date,
                CreatedAt = DateTime.Now,
                State = OrderState.Placed
            };
            foreach (var entry in itemsWithQty)
            {
                var oi = new OrderItem { Id = Guid.NewGuid(), MenuItemId =
                entry.MenuItem.Id, Quantity = entry.Quantity, UnitPrice =
                entry.MenuItem.Price };
                order.Items.Add(oi);
            }
            order.Total = total;
            _db.Orders.Add(order);
            // Decrement stock with concurrency-safe update
            foreach (var entry in itemsWithQty)
            {
                var stock = await _db.MenuItemStocks.FirstAsync(s =>
                s.MenuItemId == entry.MenuItem.Id && s.Date == today, cancellationToken);
                stock.Remaining -= entry.Quantity;
                _db.MenuItemStocks.Update(stock);
            }
            // Debit parent wallet
            parent.WalletBalance -= total;
            _db.Parents.Update(parent);
            // Persist
            await _db.SaveChangesAsync(cancellationToken);
            // Record idempotency
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                await _idem.SaveAsync(request.IdempotencyKey, order.Id);
            }
            await tx.CommitAsync(cancellationToken);
            //_log.LogInformation("Order created {OrderId} total={Total} parent={ParentId}", order.Id, total, parent.Id);
            return new CreateOrderResult { OrderId = order.Id, Total = total };
        }
        catch (DbUpdateConcurrencyException)
        {
            //_log.LogWarning(ex, "Concurrency failure while reserving stock");
            throw new Exception("Could not reserve stock due to concurrent changes. Please retry.");
        }
    }
}

