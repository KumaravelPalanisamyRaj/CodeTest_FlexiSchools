using SchoolCanteensPersistence;
using SchoolCanteensDomain;

namespace SchoolCanteensInfrastructure;

public interface IIdempotencyService
{
    Task<IdempotencyEntry?> GetAsync(string key);
    Task SaveAsync(string key, Guid orderId);
}

public class DbIdempotencyService : IIdempotencyService
{
    private readonly SchoolCanteensDbContext _db;
    public DbIdempotencyService(SchoolCanteensDbContext db) { _db = db; }
    public Task<IdempotencyEntry?> GetAsync(string key) =>
    _db.IdempotencyEntries.FindAsync(key).AsTask().ContinueWith(t => t.Result);
    public async Task SaveAsync(string key, Guid orderId)
    {
        var entry = new IdempotencyEntry
        {
            Key = key,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };
        _db.IdempotencyEntries.Add(entry);
        await _db.SaveChangesAsync();
    }
}
