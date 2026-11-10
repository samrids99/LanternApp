using Lantern.Infrastructure.Persistence.Seed;
using Xunit;

namespace Lantern.IntegrationTests.Persistence;

public class RegionSeedTests
{
  [Fact]
  public void Every_region_has_a_name()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    Assert.All(regions, r =>
        Assert.False(
            string.IsNullOrWhiteSpace(r.Name),
            $"Region with Id {r.Id} has no name."));
  }

  [Fact]
  public void Every_region_has_valid_coordinates()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    Assert.All(regions, r =>
    {
      Assert.True(
              r.CentroidLat is >= -90 and <= 90,
              $"Region {r.Id} '{r.Name}' has invalid latitude {r.CentroidLat} (expected -90 to 90).");

      Assert.True(
              r.CentroidLng is >= -180 and <= 180,
              $"Region {r.Id} '{r.Name}' has invalid longitude {r.CentroidLng} (expected -180 to 180).");
    });
  }

  [Fact]
  public void Every_region_has_a_unique_id()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    Assert.All(regions, r =>
    {
      // assert all regions have an id
      Assert.True(
              r.Id > 0,
              $"Region with name '{r.Name}' has an invalid Id {r.Id} (expected > 0).");
    });
  }

  [Fact]
  public void Every_parent_region_id_points_to_a_valid_id()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    Assert.All(regions, r =>
    {
      // assert all regions have a valid parent region id that exists in the list of regions
      Assert.True(
              r.ParentRegionId > 0 || r.ParentRegionId == null,
              $"Region with name '{r.Name}' has an invalid ParentRegionId {r.ParentRegionId} (expected > 0).");
      Assert.True(
              regions.Any(x => x.Id == r.ParentRegionId || r.ParentRegionId == null),
              $"Region with name '{r.Name}' has a ParentRegionId {r.ParentRegionId} that does not point to a valid region.");
    });
  }

  [Fact]
  public void Required_region_types_have_a_parent_region_id()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    // if a region is a 'Country' it should not have a ParentRegionId
    // RegionType is not comparable to int, so we use the underlying value of the enum (1 for Country)
    var countryRegions = regions.Where(r => (int)r.Type == 1);
    Assert.All(countryRegions, r =>
    {
      Assert.True(
              r.ParentRegionId == null,
              $"Country region with name '{r.Name}' has a ParentRegionId {r.ParentRegionId} (expected null).");
    });

    // all others should have a ParentRegionId
    var nonCountryRegions = regions.Where(r => (int)r.Type != 1);
    Assert.All(nonCountryRegions, r =>
    {
      Assert.True(
              r.ParentRegionId > 0,
              $"Region with name '{r.Name}' has an invalid ParentRegionId {r.ParentRegionId} (expected > 0).");
    });
  }

  [Fact]
  public void Region_hierarchy_should_not_exceed_three_levels()
  {
    var regions = RegionSeedLoader.Load();

    Assert.NotEmpty(regions);

    // Counting hops up the tree: country = 0, area = 1, locality = 2.
    const int maxHops = 2;

    // One lookup table instead of a FirstOrDefault scan on every hop.
    var byId = regions.ToDictionary(r => r.Id);

    Assert.All(regions, r =>
    {
      var hops = 0;
      var current = r;

      // "is int parentId" is true only when the value isn't null,
      // and gives us the plain int to use.
      while (current.ParentRegionId is int parentId)
      {
        if (!byId.TryGetValue(parentId, out var parent))
        {
          Assert.Fail(
              $"Region {current.Id} '{current.Name}' has ParentRegionId {parentId}, which does not exist.");
        }

        hops++;

        if (hops > maxHops)
        {
          Assert.Fail(
              $"Region {r.Id} '{r.Name}' is more than {maxHops} levels below a country (a deeper chain, or a parent loop).");
        }

        current = parent;
      }
    });
  }
}
