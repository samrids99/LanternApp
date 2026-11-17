namespace Lantern.Application.Regions;

// interface returns a DTO as to not expose unnecessary details of the domain model to the API layer. The DTO is also more efficient to serialize and send over the wire.
public interface IRegionRepository
{
  /// <summary>
  /// Gets every region, ordered by <see cref="RegionDto.Id"/> ascending.
  /// </summary>
  /// <remarks>
  /// The order is part of the contract: clients and the ETag rely on the
  /// list being the same every time unless the data changes.
  /// </remarks>
  /// <param name="cancellationToken">
  /// Stops the query if the caller gives up, for example when the HTTP
  /// request is aborted.
  /// </param>
  /// <returns>All regions, or an empty list if there are none.</returns>
  Task<IReadOnlyList<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
