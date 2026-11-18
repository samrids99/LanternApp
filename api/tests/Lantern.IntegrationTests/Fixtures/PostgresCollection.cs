using Xunit;

namespace Lantern.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
