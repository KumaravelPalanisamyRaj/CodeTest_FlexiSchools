
using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolCanteensInfrastructure;
using SchoolCanteensPersistence;

var builder = WebApplication.CreateBuilder(args);

// Add the custom JSON file with reloadOnChange = true
builder.Configuration.AddJsonFile("appsettings.custom.json", optional: false, reloadOnChange: true);

// Configure settings from appsettings.json
builder.Services.Configure<CanteenSettings>(builder.Configuration.GetSection("CanteenSettings"));

// MediatR
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
// Api services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (CanteenConfig.Instance.UseInMemoryData)
{
    builder.Services.AddDbContext<SchoolCanteensDbContext>(opts =>
    opts.UseInMemoryDatabase("SchoolCanteen"));
}
else
{
    builder.Services.AddDbContext<SchoolCanteensDbContext>(opt =>
    opt.UseSqlServer(CanteenConfig.Instance.DefaultDataConnection));
}
// Simple Idempotency store service
builder.Services.AddScoped<IIdempotencyService, DbIdempotencyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Ensure DB created for demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolCanteensDbContext>();
    db.Database.EnsureCreated();
    // Optionally seed demo data here
    await DemoData.SeedAsync(db);
}
app.Run();

