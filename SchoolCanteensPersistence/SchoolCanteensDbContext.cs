

using Microsoft.EntityFrameworkCore;
using SchoolCanteensDomain;

namespace SchoolCanteensPersistence;

public class SchoolCanteensDbContext : DbContext
{
    public SchoolCanteensDbContext(DbContextOptions<SchoolCanteensDbContext> opts) : base(opts)
    { }
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Canteen> Canteens => Set<Canteen>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemStock> MenuItemStocks => Set<MenuItemStock>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<IdempotencyEntry> IdempotencyEntries =>
    Set<IdempotencyEntry>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parent>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p =>
            p.WalletBalance).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<Student>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasOne(s => s.Parent).WithMany(p =>
            p.Students).HasForeignKey(s => s.ParentId);
        });
        modelBuilder.Entity<Canteen>(b =>
        {
            b.HasKey(c => c.Id);
        });
        modelBuilder.Entity<MenuItem>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Price).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<MenuItemStock>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property<byte[]?>("RowVersion").IsRowVersion();
            b.HasIndex(s => new { s.MenuItemId, s.Date }).IsUnique();
        });
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Total).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<IdempotencyEntry>(b =>
        {
            b.HasKey(i => i.Key);
        });
    }
}