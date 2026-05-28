using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;

namespace TestApp;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TenantEntity> Tenants { get; set; }
    public DbSet<PostEntity> Posts { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Convert (unsupported) JsonNode into JsonElement to store as `jsonb` in PostgreSQL
        configurationBuilder.Properties<JsonNode>().HaveConversion<JsonNodeValueConverter>();
    }
}

[Table("tenant")]
[PrimaryKey(nameof(Id))]
public sealed class TenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Name { get; set; }
    public required JsonNode MetaData { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public List<PostEntity> Posts { get; set; } = null!;
}

[Table("post")]
[PrimaryKey(nameof(Id))]
public sealed class PostEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; }

    public required string Title { get; set; }
    public required string Content { get; set; }
    public required JsonDocument MetaData { get; set; }
    public required JsonElement TagData { get; set; }

    public required Guid TenantId { get; set; }

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public TenantEntity Tenant { get; set; } = null!;
}
