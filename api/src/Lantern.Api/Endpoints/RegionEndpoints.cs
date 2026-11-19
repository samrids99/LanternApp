using Lantern.Application.Regions;

namespace Lantern.Api.Endpoints;

public static class RegionEndpoints
{
    public static IEndpointRouteBuilder MapRegionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/regions");
        group.MapGet("/", GetAll);
        return app;
    }

    private static async Task<IResult> GetAll(IRegionRepository regions, CancellationToken ct)
        => Results.Ok(await regions.GetAllAsync(ct));
}
