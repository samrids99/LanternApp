using Lantern.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Lantern.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    // Same major version as the dev database, so tests run against
    // what the app really uses.
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:17").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Applies InitialCreate (and any later migrations), not
        // EnsureCreated, so the test schema matches production's.
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    // A NEW context every time, so each step of a test sees the database
    // as it really is, not a context's tracked copy of it.
    public LanternDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LanternDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new LanternDbContext(options);
    }

    // Empties the table so each test starts from a known state.
    // CASCADE handles the self-referencing parent foreign key.
    public async Task ResetAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Regions\" CASCADE;");
    }
}
