using Lantern.Domain.Regions;
using Lantern.Infrastructure.Persistence.Seed;
using Lantern.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Lantern.IntegrationTests.Persistence.Seed;

// IAsyncLifetime: xUnit makes a new instance per test, so InitializeAsync
// runs before EACH test and gives it an empty table.
[Collection(PostgresCollection.Name)]
public class RegionSeederTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public RegionSeederTests(PostgresFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Seeding_an_empty_database_inserts_every_region()
    {
        var regions = RegionSeedLoader.Load();

        var result = await SeedAsync(regions);

        Assert.Equal(regions.Count, result.Inserted);
        Assert.Equal(0, result.Updated);
    }

    [Fact]
    public async Task Seeding_twice_does_not_create_duplicates()
    {
        await SeedAsync(RegionSeedLoader.Load());
        await SeedAsync(RegionSeedLoader.Load());

        await using var db = _fixture.CreateContext();
        Assert.Equal(RegionSeedLoader.Load().Count, await db.Regions.CountAsync());
    }

    [Fact]
    public async Task A_changed_value_in_the_file_updates_the_existing_row()
    {
        var regions = RegionSeedLoader.Load();
        await SeedAsync(regions);

        var original = regions.First(r => r.Type == RegionType.Locality);
        var changed = regions
            .Select(r => r.Id == original.Id ? Copy(r, lat: original.CentroidLat + 1) : Copy(r))
            .ToList();

        var result = await SeedAsync(changed);

        await using var db = _fixture.CreateContext();
        var stored = await db.Regions.SingleAsync(r => r.Id == original.Id);
        Assert.Equal(new RegionSeedResult(0, 1), result);
    }

    [Fact]
    public async Task The_order_of_the_input_does_not_matter()
    {
        // Localities first, countries last: a child would arrive before
        // its parent if the seeder did not sort by depth.
        var reversed = RegionSeedLoader.Load().Reverse().ToList();

        await SeedAsync(reversed);

        await using var db = _fixture.CreateContext();
        Assert.Equal(reversed.Count, await db.Regions.CountAsync());
    }

    [Fact]
    public async Task A_region_missing_from_the_input_is_never_deleted()
    {
        var regions = RegionSeedLoader.Load();
        await SeedAsync(regions);

        // Seed again with the last region removed from the input.
        var removed = regions.Last();
        var without = regions.Where(r => r.Id != removed.Id).Select(r => Copy(r)).ToList();

        await SeedAsync(without);

        await using var db = _fixture.CreateContext();
        Assert.True(
            await db.Regions.AnyAsync(r => r.Id == removed.Id),
            $"Region {removed.Id} '{removed.Name}' was deleted, but the seeder must never delete.");
    }

    [Fact]
    public async Task Parent_links_are_preserved()
    {
        var regions = RegionSeedLoader.Load();
        await SeedAsync(regions);

        await using var db = _fixture.CreateContext();
        var stored = await db.Regions.ToDictionaryAsync(r => r.Id);

        Assert.All(regions, expected =>
            Assert.True(
                expected.ParentRegionId == stored[expected.Id].ParentRegionId,
                $"Region {expected.Id} '{expected.Name}' should have parent " +
                $"{expected.ParentRegionId?.ToString() ?? "none"}, " +
                $"but has {stored[expected.Id].ParentRegionId?.ToString() ?? "none"}."));
    }

    [Fact]
    public async Task Seeding_unchanged_data_a_second_time_writes_nothing()
    {
        await SeedAsync(RegionSeedLoader.Load());

        var second = await SeedAsync(RegionSeedLoader.Load());

        Assert.Equal(new RegionSeedResult(0, 0), second);
    }

    [Fact]
    public async Task A_parent_loop_throws_and_writes_nothing()
    {
        // Two regions that are each other's parent: a loop.
        var loopA = new Region { Id = 1, Name = "LoopA", Type = RegionType.Locality, CentroidLat = 0, CentroidLng = 0, ParentRegionId = 2 };
        var loopB = new Region { Id = 2, Name = "LoopB", Type = RegionType.Locality, CentroidLat = 0, CentroidLng = 0, ParentRegionId = 1 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => SeedAsync(new List<Region> { loopA, loopB }));

        // Both names start with "Loop", so this passes whichever region the message names.
        Assert.Contains("Loop", ex.Message);

        // The check runs before any insert, so nothing should have been written.
        await using var db = _fixture.CreateContext();
        Assert.Equal(0, await db.Regions.CountAsync());
    }

    [Fact]
    public async Task A_missing_parent_throws_and_writes_nothing()
    {
        var orphan = new Region { Id = 1, Name = "Orphan", Type = RegionType.Locality, CentroidLat = 0, CentroidLng = 0, ParentRegionId = 999 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => SeedAsync(new List<Region> { orphan }));

        // The message names the region and the parent ID it couldn't find.
        Assert.Contains("Orphan", ex.Message);
        Assert.Contains("999", ex.Message);

        await using var db = _fixture.CreateContext();
        Assert.Equal(0, await db.Regions.CountAsync());
    }

    // --- helpers ---

    // Each seed run gets its own context and seeder, like a real startup.
    private async Task<RegionSeedResult> SeedAsync(IReadOnlyList<Region> regions)
    {
        await using var db = _fixture.CreateContext();
        var seeder = new RegionSeeder(db, NullLogger<RegionSeeder>.Instance);
        return await seeder.SeedAsync(regions);
    }

    // Region has required init properties and no "with", so edit a copy by
    // building a new one. Also avoids reusing an object a context has tracked.
    private static Region Copy(Region r, double? lat = null) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Type = r.Type,
        ParentRegionId = r.ParentRegionId,
        CentroidLat = lat ?? r.CentroidLat,
        CentroidLng = r.CentroidLng
    };
}
