using Lantern.Domain.Regions;

namespace Lantern.Application.Regions;

public sealed record RegionDto(
  int Id,
  string Name,
  // Type is serialised as its name ("Country", "Area", "Locality").
  // Renaming or removing a RegionType member is a breaking API change.
  RegionType Type,
  int? ParentRegionId,
  double CentroidLat,
  double CentroidLng
);
