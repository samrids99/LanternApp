using Lantern.Application.Regions;
using Lantern.Domain.Regions;
using Lantern.Infrastructure.Persistence;
using Lantern.IntegrationTests.Fixtures;

namespace Lantern.IntegrationTests.Persistence;

[Collection(PostgresCollection.Name)]
public class RegionRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    public RegionRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // Order test: insert regions with no parent, using separate
    // SaveChanges calls and Ids that aren’t in insertion order
    // (say 3, then 1, then 2). Assert the DTOs come back as 1, 2, 3.
    [Fact]
    public async Task GetAllAsync_returns_regions_in_id_order()
    {
        var regions = new List<Region>
    {
        new Region
        {
          Id = 3,
          Name = "Region 3",
          Type = RegionType.Locality,
          CentroidLat = 0,
          CentroidLng = 0
        },
        new Region
        {
          Id = 1,
          Name = "Region 1",
          Type = RegionType.Locality,
          CentroidLat = 0,
          CentroidLng = 0
        },
        new Region
        {
          Id = 2,
          Name = "Region 2",
          Type = RegionType.Locality,
          CentroidLat = 0,
          CentroidLng = 0
        }
    };

        await using (var db = _fixture.CreateContext())
        {
            foreach (var region in regions)
            {
                db.Regions.Add(region);
                await db.SaveChangesAsync();
            }
        }

        await using var db2 = _fixture.CreateContext();
        var repository = new RegionRepository(db2);
        var result = await repository.GetAllAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(r => r.Id));
    }

    // Mapping test: save a child with a parent and check every DTO field,
    // including Type and ParentRegionId
    [Fact]
    public async Task GetAllAsync_returns_all_fields_correctly()
    {
        var parent = new Region
        {
            Id = 1,
            Name = "Parent",
            Type = RegionType.Country,
            CentroidLat = 1,
            CentroidLng = 2
        };
        var child = new Region
        {
            Id = 2,
            Name = "Child",
            Type = RegionType.Area,
            ParentRegionId = parent.Id,
            CentroidLat = 3,
            CentroidLng = 4
        };

        await using (var db = _fixture.CreateContext())
        {
            db.Regions.Add(parent);
            db.Regions.Add(child);
            await db.SaveChangesAsync();
        }

        await using var db2 = _fixture.CreateContext();
        var repository = new RegionRepository(db2);
        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        var parentDto = result.Single(r => r.Id == parent.Id);
        Assert.Equal(parent.Name, parentDto.Name);
        Assert.Equal(parent.Type, parentDto.Type);
        Assert.Null(parentDto.ParentRegionId);
        Assert.Equal(parent.CentroidLat, parentDto.CentroidLat);
        Assert.Equal(parent.CentroidLng, parentDto.CentroidLng);

        var childDto = result.Single(r => r.Id == child.Id);
        Assert.Equal(child.Name, childDto.Name);
        Assert.Equal(child.Type, childDto.Type);
        Assert.Equal(child.ParentRegionId, childDto.ParentRegionId);
        Assert.Equal(child.CentroidLat, childDto.CentroidLat);
        Assert.Equal(child.CentroidLng, childDto.CentroidLng);
    }

    // Empty test: an empty table returns an empty list, not null and not an exception.
    [Fact]
    public async Task GetAllAsync_returns_empty_list_for_empty_table()
    {
        await using var db = _fixture.CreateContext();
        var repository = new RegionRepository(db);
        var result = await repository.GetAllAsync();
        Assert.Empty(result);
    }
}
