using System.Net;
using System.Text.Json;
using Lantern.Domain.Regions;
using Lantern.IntegrationTests.Fixtures;

namespace Lantern.IntegrationTests.Api;

[Collection(PostgresCollection.Name)]
public class RegionEndpointsTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private LanternApiFactory _factory = null!;   // created in InitializeAsync, before any test runs

    public RegionEndpointsTests(PostgresFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
        _factory = new LanternApiFactory(_fixture.ConnectionString);
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    // Arrange through the database directly, so the test states exactly what data exists.
    private async Task AddRegionsAsync(params Region[] regions)
    {
        await using var db = _fixture.CreateContext();
        db.Regions.AddRange(regions);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetRegions_returns_200_and_a_json_array()
    {
        await AddRegionsAsync(
            new Region { Id = 1, Name = "England", Type = RegionType.Country, CentroidLat = 52.9, CentroidLng = -2.3 });

        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/regions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        // Parse the raw JSON rather than deserialising into RegionDto: a DTO would accept
        // either shape of enum and hide exactly what this suite exists to check.
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetRegions_serialises_type_as_its_name()
    {
        await AddRegionsAsync(
            new Region { Id = 1, Name = "England", Type = RegionType.Country, CentroidLat = 52.9, CentroidLng = -2.3 },
            new Region { Id = 2, Name = "Peak District", Type = RegionType.Area, ParentRegionId = 1, CentroidLat = 53.3, CentroidLng = -1.8 });

        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/regions");
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Ordered by Id (the repository's contract), so index 1 is the Peak District.
        var type = doc.RootElement[1].GetProperty("type");
        Assert.Equal(JsonValueKind.String, type.ValueKind);
        Assert.Equal("Area", type.GetString());
    }

    [Fact]
    public async Task GetRegions_returns_an_empty_array_when_there_are_no_regions()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/regions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetRegions_serialises_parentRegionId_as_null_or_a_number()
    {
        await AddRegionsAsync(
            new Region { Id = 1, Name = "England", Type = RegionType.Country, CentroidLat = 52.9, CentroidLng = -2.3 },
            new Region { Id = 2, Name = "Peak District", Type = RegionType.Area, ParentRegionId = 1, CentroidLat = 53.3, CentroidLng = -1.8 });

        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/regions");
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Ordered by Id: index 0 is the country, index 1 the area.
        var countryParent = doc.RootElement[0].GetProperty("parentRegionId");
        var areaParent = doc.RootElement[1].GetProperty("parentRegionId");

        Assert.Equal(JsonValueKind.Null, countryParent.ValueKind);
        Assert.Equal(JsonValueKind.Number, areaParent.ValueKind);
        Assert.Equal(1, areaParent.GetInt32());
    }
}
