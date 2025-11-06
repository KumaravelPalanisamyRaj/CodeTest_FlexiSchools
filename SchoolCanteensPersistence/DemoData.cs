
using SchoolCanteensDomain;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace SchoolCanteensPersistence;

public static class DemoData
{
    public static async Task SeedAsync(SchoolCanteensDbContext db)
    {
        if (await db.Orders.AnyAsync())
            return;

        // Create parents
        var parent1 = new Parent { Id = Guid.NewGuid(), Name = "Alice Smith", Email = "alice@example.com", WalletBalance = 50m };
        var parent2 = new Parent { Id = Guid.NewGuid(), Name = "Bob Johnson", Email = "bob@example.com", WalletBalance = 30m };

        // Create students
        var student1 = new Student { Id = Guid.NewGuid(), Name = "Charlie Smith", Parent = parent1, ParentId = parent1.Id };
        var student2 = new Student { Id = Guid.NewGuid(), Name = "Daisy Johnson", Parent = parent2, ParentId = parent2.Id };
        parent1.Students.Add(student1);
        parent2.Students.Add(student2);

        // Create canteens
        var canteen1 = new Canteen { Id = Guid.NewGuid(), Name = "Main Canteen", OpeningDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday }, CutoffTime = new TimeSpan(10, 30, 0) };

        // Create menu items
        var item1 = new MenuItem { Id = Guid.NewGuid(), Name = "Sandwich", Price = 5.5m, Canteen = canteen1, CanteenId = canteen1.Id };
        var item2 = new MenuItem { Id = Guid.NewGuid(), Name = "Juice", Price = 2.5m, Canteen = canteen1, CanteenId = canteen1.Id };

        // Create orders
        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            Parent = parent1,
            ParentId = parent1.Id,
            Student = student1,
            StudentId = student1.Id,
            Canteen = canteen1,
            CanteenId = canteen1.Id,
            FulfilmentDate = DateTime.Today,
            CreatedAt = DateTime.Now,
            State = OrderState.Placed,
            Total = item1.Price + item2.Price,
            Items = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), MenuItem = item1, MenuItemId = item1.Id, Quantity = 1, UnitPrice = item1.Price },
                new OrderItem { Id = Guid.NewGuid(), MenuItem = item2, MenuItemId = item2.Id, Quantity = 1, UnitPrice = item2.Price }
            }
        };

        // Add all entities to the context
        await db.Parents.AddRangeAsync(parent1, parent2);
        await db.Students.AddRangeAsync(student1, student2);
        await db.Canteens.AddAsync(canteen1);
        await db.MenuItems.AddRangeAsync(item1, item2);
        await db.Orders.AddAsync(order1);

        await db.SaveChangesAsync();
    }
}