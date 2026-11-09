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
}
