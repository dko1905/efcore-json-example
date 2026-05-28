using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connString =
    "Host=localhost:5432;Database=main;Username=postgres;Password=password123$;Include Error Detail=true";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

const string TENANT_NAME = "test";

app.MapGet(
    "/",
    async (HttpContext httpContext, AppDbContext dbContext) =>
    {
        return await dbContext.Tenants.Include(t => t.Posts).ToListAsync();
    }
);

app.MapGet(
    "/create",
    async (HttpContext httpContext, AppDbContext dbContext) =>
    {
        if (await dbContext.Tenants.AnyAsync(t => t.Name == "test"))
            return "OK - Exists";

        var tenant = await dbContext.Tenants.AddAsync(
            new() { Name = "test", MetaData = JsonNode.Parse("""{"foo": "bar"}""")! }
        );
        await dbContext.Posts.AddAsync(
            new()
            {
                Title = TENANT_NAME,
                Content = "hello world...",
                MetaData = JsonDocument.Parse("""{"slug": "hello-world"}"""),
                TagData = JsonElement.Parse("""{"tags": ["a", "b"]}"""),
                TenantId = tenant.Entity.Id,
            }
        );

        var sw = Stopwatch.StartNew();
        await dbContext.SaveChangesAsync();
        return $"OK - Created {sw.Elapsed}";
    }
);

app.MapGet(
    "/update",
    async (HttpContext httpContext, AppDbContext dbContext) =>
    {
        var tenant = await dbContext.Tenants.AsTracking().FirstAsync(t => t.Name == TENANT_NAME);
        if (tenant is null)
            return "OK - Does not exist";

        // ugly hack in order to trigger change detection
        var metadataCopy = tenant.MetaData.DeepClone();
        metadataCopy["count"] = Random.Shared.Next() % 256;
        tenant.MetaData = metadataCopy;

        await dbContext.SaveChangesAsync();
        return $"OK - Updated {JsonSerializer.Serialize(tenant)}";
    }
);

app.MapGet(
    "/migrate",
    async (HttpContext httpContext, AppDbContext dbContext) =>
    {
        await dbContext.Database.MigrateAsync();
        return "OK - Migrated";
    }
);

app.Run();
