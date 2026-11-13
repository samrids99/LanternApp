using Lantern.Domain.Regions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lantern.Infrastructure.Persistence.Seed;

// internal: only Infrastructure (and the integration tests, via
// InternalsVisibleTo) can see it. Api goes through the public extension
// method further down.
internal sealed class RegionSeeder
{
  private readonly LanternDbContext _db;
  private readonly ILogger<RegionSeeder> _logger;

  // Both arrive through dependency injection.
  public RegionSeeder(LanternDbContext db, ILogger<RegionSeeder> logger)
  {
    _db = db;
    _logger = logger;
  }

  // The list is a parameter so a test can pass its own data (reversed,
  // edited, and so on). The default is the embedded file.
  public async Task SeedAsync(
      IReadOnlyList<Region>? regions = null,
      CancellationToken ct = default)
  {
    regions ??= RegionSeedLoader.Load();

    // 1. Sort by depth so parents are always handled before children.
    //    The file's row order no longer matters.
    var ordered = OrderByDepth(regions);

    // 2. Load what's already in the database, keyed by ID.
    //    Deliberately TRACKED (no AsNoTracking): EF remembers each row's
    //    original values, which is how it spots what has changed.
    var existing = await _db.Regions.ToDictionaryAsync(r => r.Id, ct);

    // 3. For each region in the file: insert it, or update the existing row.
    foreach (var incoming in ordered)
    {
      if (existing.TryGetValue(incoming.Id, out var current))
      {
        // Copies the incoming scalar values (name, type, coordinates,
        // parent ID) onto the tracked entity. EF marks it Modified only
        // if something actually differs.
        _db.Entry(current).CurrentValues.SetValues(incoming);
      }
      else
      {
        _db.Regions.Add(incoming);
      }
    }

    // 4. Never delete. If the database holds regions the file no longer
    //    lists, leave them alone but say so.
    var fileIds = ordered.Select(r => r.Id).ToHashSet();
    var drifted = existing.Keys.Where(id => !fileIds.Contains(id)).ToList();
    if (drifted.Count > 0)
    {
      _logger.LogWarning(
          "{Count} region(s) exist in the database but not in regions.json " +
          "(IDs: {Ids}). They were left untouched.",
          drifted.Count, string.Join(", ", drifted));
    }

    // 5. Count what EF is about to do, then save everything in one go.
    var inserted = _db.ChangeTracker.Entries<Region>()
        .Count(e => e.State == EntityState.Added);
    var updated = _db.ChangeTracker.Entries<Region>()
        .Count(e => e.State == EntityState.Modified);

    await _db.SaveChangesAsync(ct);

    _logger.LogInformation(
        "Region seed complete: {Inserted} inserted, {Updated} updated.",
        inserted, updated);
  }

  private static List<Region> OrderByDepth(IReadOnlyList<Region> regions)
  {
    // Throws on a duplicate ID, which is what we want for bad seed data.
    var byId = regions.ToDictionary(r => r.Id);

    // "hops" up the tree: country 0, area 1, locality 2.
    // a local function can consume variables from the outer method, so it can see byId and regions.
    int Depth(Region region)
    {
      var hops = 0;
      var current = region;

      while (current.ParentRegionId is int parentId)
      {
        if (!byId.TryGetValue(parentId, out var parent))
        {
          throw new InvalidOperationException(
              $"Region {current.Id} '{current.Name}' has ParentRegionId " +
              $"{parentId}, which is not in the seed data.");
        }

        // A parent loop would never end, so cap the walk at the number
        // of regions: no valid chain can be longer than that.
        if (++hops > regions.Count)
        {
          throw new InvalidOperationException(
              $"Region {region.Id} '{region.Name}' is part of a parent loop.");
        }

        current = parent;
      }

      return hops;
    }

    return regions.OrderBy(Depth).ToList();
  }
}
