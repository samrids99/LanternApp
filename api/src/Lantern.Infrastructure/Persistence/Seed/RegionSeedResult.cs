namespace Lantern.Infrastructure.Persistence.Seed;

// A record gives you value equality for free, so a test can compare
// two results with Assert.Equal(new RegionSeedResult(0, 0), result).
// internal, like the seeder: only Infrastructure and the tests see it.
internal sealed record RegionSeedResult(int Inserted, int Updated);
