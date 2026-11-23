using Lantern.Domain.Regions;
using Lantern.Infrastructure.Persistence.Seed;
using Xunit;

namespace Lantern.IntegrationTests.Persistence;

public class RegionSeedTests
{
    // Max separation set as 50km, most towns are within 20km of their parent region,
    // but some are further away. This is a reasonable limit to catch any data entry errors.
    private const double MaxSeparationBetweenLocalityAndAreaKm = 50.0;

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
    public void Every_region_has_a_Valid_id()
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
        var countryRegions = regions.Where(r => r.Type == RegionType.Country);
        Assert.All(countryRegions, r =>
        {
            Assert.True(
                r.ParentRegionId == null,
                $"Country region with name '{r.Name}' has a ParentRegionId {r.ParentRegionId} (expected null).");
        });

        // all others should have a ParentRegionId
        var nonCountryRegions = regions.Where(r => r.Type != RegionType.Country);
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

    [Fact]
    public void Localities_should_be_within_reasonable_distance_of_their_parent_regions()
    {
        var regions = RegionSeedLoader.Load();

        Assert.NotEmpty(regions);

        var byId = regions.ToDictionary(r => r.Id);

        // if region is a 'Locality' it should be within 50km of its parent region
        var localityRegions = regions.Where(r => r.Type == RegionType.Locality);
        Assert.NotEmpty(localityRegions);
        Assert.All(localityRegions, r =>
        {
            if (r.ParentRegionId is not int parentId)
            {
                Assert.Fail(
                    $"Locality region {r.Id} '{r.Name}' has no ParentRegionId.");
                return;
            }

            if (!byId.TryGetValue(parentId, out var parent))
            {
                Assert.Fail(
                    $"Locality region {r.Id} '{r.Name}' has ParentRegionId {r.ParentRegionId}, which does not exist.");
                return;
            }

            var distance = HaversineDistance(r.CentroidLat, r.CentroidLng, parent.CentroidLat, parent.CentroidLng);

            Assert.True(
                distance <= MaxSeparationBetweenLocalityAndAreaKm,
                $"Locality region {r.Id} '{r.Name}' is {distance:F2} km away from its parent region {parent.Id} '{parent.Name}', which exceeds the {MaxSeparationBetweenLocalityAndAreaKm} km limit.");
        });

    }

    // Using the Haversine formula to calculate the distance between two points on the Earth.
    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Radius of the Earth in kilometers
        var dLat = (lat2 - lat1) * (Math.PI / 180);
        var dLon = (lon2 - lon1) * (Math.PI / 180);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // Distance in kilometers
    }
}
