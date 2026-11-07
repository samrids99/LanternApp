
namespace Lantern.Domain.Regions;

public class Region
{
  public int Id { get; init; }
  public required string Name { get; init; }
  public required RegionType Type { get; init; }
  // seed parent region with id, never the object
  public int? ParentRegionId { get; init; }
  public Region? ParentRegion { get; init; }
  public required double CentroidLat { get; init; }
  public required double CentroidLng { get; init; }
  public List<Region> ChildRegions { get; init; } = [];
}
