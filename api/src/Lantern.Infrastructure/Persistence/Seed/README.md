# Region seed data

The seed data for Lantern's regions. `RegionSeedLoader` reads `regions.json` (embedded
in the assembly at build time) and `RegionSeeder` writes it to the database.

## How seeding behaves

- **`regions.json` is the source of truth.** The seeder makes the database match it.
- **Update on difference.** A row whose values differ from the file is updated, and a
  warning is logged. Unchanged rows are not touched. Each run reports how many rows
  were inserted and updated.
- **Never delete.** A region removed from the file stays in the database, because
  stays and other entities may refer to it.
- **Order in the file does not matter.** The seeder sorts regions by depth itself. A
  parent that doesn't exist, or a parent loop, fails with a clear error.
- **Development only** for the MVP. Seeding in production is undecided.
- The file is embedded at build time, so changes are picked up after a rebuild
  (`dotnet run` does this for you).

## Source

Coordinates were taken from Wikipedia articles via GeoHack, accessed 9 November 2026:

| Id  | Region          | Article                                                  |
| --- | --------------- | -------------------------------------------------------- |
| 1   | England         | [England](https://en.wikipedia.org/wiki/England)         |
| 2   | Peak District   | [Peak District](https://en.wikipedia.org/wiki/Peak_District) |
| 3   | Bakewell        | [Bakewell](https://en.wikipedia.org/wiki/Bakewell)       |
| 4   | Yorkshire Dales | [Yorkshire Dales](https://en.wikipedia.org/wiki/Yorkshire_Dales) |
| 5   | Hawes           | [Hawes](https://en.wikipedia.org/wiki/Hawes)             |

## Licence and attribution

Wikipedia text is available under the
[Creative Commons Attribution-ShareAlike 4.0 licence](https://creativecommons.org/licenses/by-sa/4.0/deed.en).
The coordinates are attributed to the Wikipedia contributors of the articles above, and
this file is shared under the same licence.

## What the coordinates mean

A centroid is the approximate centre of a region. It is deliberately never the
location of a real pitch or stay (ADR 001).

## Editing rules

- IDs are permanent: never reused, never renumbered.
- Parents are referenced by ID only.
- `type` is one of the names `Country`, `Area` or `Locality` (not a number).
- Unknown property names are rejected when the file is loaded.
- Maximum depth is three levels (ADR 006).
- Do not remove a region. It may have child regions or other entities that refer to it.
- Changing a region's parent or coordinates updates the existing row on the next
  Development start.

## Adding a region

1. Use the next free ID.
2. Check the parent exists, and take the coordinates from the source above (add the
   article to the table).
3. Run `dotnet test`. A wrong parent or a typo in a coordinate is caught by the tests
   below.

## Validation

`Lantern.IntegrationTests/Persistence/RegionSeedTests.cs` checks that every region has:

- a name, a valid ID, and coordinates in range
- a parent that exists (and that only countries have none)
- a hierarchy no deeper than three levels, with no parent loops
- a locality within 50 km of its parent region

The seeder's own behaviour (insert, update on difference, never delete, order) is tested
in `Persistence/Seed/RegionSeederTests.cs`.
