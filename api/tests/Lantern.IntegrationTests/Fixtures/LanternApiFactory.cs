using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Lantern.IntegrationTests.Fixtures;

/// <summary>
/// Starts the real Lantern API in memory, pointed at the test Postgres container.
/// </summary>
public class LanternApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringKey = "ConnectionStrings:Lantern";

    private readonly string _connectionString;

    public LanternApiFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(ConnectionStringKey, _connectionString);
    }
}
