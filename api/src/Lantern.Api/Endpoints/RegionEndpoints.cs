using Lantern.Application.Regions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lantern.Api.Endpoints;

public static class RegionEndpoints
{
    public static IEndpointRouteBuilder MapRegionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/regions");
        group.MapGet("/", GetAll);
        return app;
    }

    private static async Task<Ok<IReadOnlyList<RegionDto>>> GetAll(
        IRegionRepository regions,
        CancellationToken ct)
        => TypedResults.Ok(await regions.GetAllAsync(ct));
}
