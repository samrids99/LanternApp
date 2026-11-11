using Lantern.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lantern.Infrastructure;

public static class DependencyInjection
{
  // "this" makes it an extension method, so Program.cs can write
  // builder.Services.AddInfrastructure(...) as if it were built in.
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // Reads the "ConnectionStrings:Lantern".
    // Returns null if the key is missing.
    var connectionString = configuration.GetConnectionString("Lantern")
        ?? throw new InvalidOperationException(
            "Connection string 'Lantern' was not found. " +
            "Set it with: dotnet user-secrets set \"ConnectionStrings:Lantern\" \"...\" --project src/Lantern.Api");

    // Registers LanternDbContext so the framework can create it and
    // pass in the options (the constructor you already wrote).
    services.AddDbContext<LanternDbContext>(options =>
        options.UseNpgsql(connectionString));

    return services;
  }
}
