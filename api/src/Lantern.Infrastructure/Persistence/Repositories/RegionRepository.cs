using Lantern.Application.Regions;
using Microsoft.EntityFrameworkCore;

namespace Lantern.Infrastructure.Persistence;

internal sealed class RegionRepository : IRegionRepository
{
    private readonly LanternDbContext _dbContext;

    public RegionRepository(LanternDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var regions = await _dbContext.Regions
          .OrderBy(r => r.Id)
          .Select(r => new RegionDto
          (
            r.Id,
            r.Name,
            r.Type,
            r.ParentRegionId,
            r.CentroidLat,
            r.CentroidLng
          ))
          .ToListAsync(cancellationToken);

        return regions;
    }
}
