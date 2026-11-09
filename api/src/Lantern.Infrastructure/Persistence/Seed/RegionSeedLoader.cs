using System.Text.Json;
using System.Text.Json.Serialization;
using Lantern.Domain.Regions;

namespace Lantern.Infrastructure.Persistence.Seed;

// internal: only visible inside Infrastructure (and the integration tests,
// via InternalsVisibleTo). Other layers can't depend on how seeding works.
// static: the loader holds no state, so there's nothing to construct.
internal static class RegionSeedLoader
{
  // An embedded resource's name is built as:
  //   <root namespace>.<folders separated by dots>.<file name>
  // If you move the file or rename the project, this string must change too.
  private const string ResourceName =
      "Lantern.Infrastructure.Persistence.Seed.regions.json";

  // static readonly: created once and reused. JsonSerializerOptions caches
  // type metadata internally, so a new instance per call is wasteful.
  private static readonly JsonSerializerOptions Options = new()
  {
    // "name", "Name" and "NAME" all match the Name property.
    PropertyNameCaseInsensitive = true,

    // JSON names are camelCase when writing (relevant if you ever export).
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

    // A property in the file that Region doesn't have is an error,
    // which catches typos like "centroidLatt".
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,

    // Types are written as "Country", "Area", "Locality".
    // allowIntegerValues: false rejects a bare 1, 2 or 3, so reordering
    // the enum can never silently change what a row means.
    Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
  };

  public static IReadOnlyList<Region> Load()
  {
    // Every type knows which assembly it was compiled into.
    // The embedded file lives in that assembly.
    var assembly = typeof(RegionSeedLoader).Assembly;

    // Returns null if no resource has that name.
    // "using var" disposes the stream when the method ends.
    using var stream = assembly.GetManifestResourceStream(ResourceName);

    if (stream is null)
    {
      // List what actually exists, so a wrong name is easy to fix.
      var available = string.Join(", ", assembly.GetManifestResourceNames());
      throw new InvalidOperationException(
          $"Embedded resource '{ResourceName}' not found. Available: [{available}]");
    }

    // Throws JsonException on bad JSON, a missing required property,
    // an unknown property or an invalid enum value. That's deliberate:
    // bad seed data should stop the app, not be quietly accepted.
    var regions = JsonSerializer.Deserialize<List<Region>>(stream, Options);

    // Deserialize returns null if the file contains the literal "null".
    if (regions is null || regions.Count == 0)
    {
      throw new InvalidOperationException(
          "regions.json was read but contains no regions.");
    }

    return regions;
  }
}
