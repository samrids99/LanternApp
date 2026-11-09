# Region seed data

The seed data for Lantern's regions, loaded at startup by `RegionSeedLoader`.

## Source

Coordinates were taken from Wikipedia articles via GeoHack, accessed 9th November 2026:

| Id  | Region          | Article                                         |
| --- | --------------- | ----------------------------------------------- |
| 1   | England         | [https://en.wikipedia.org/wiki/England]         |
| 2   | Peak District   | [https://en.wikipedia.org/wiki/Peak_District]   |
| 3   | Bakewell        | [https://en.wikipedia.org/wiki/Bakewell]        |
| 4   | Yorkshire Dales | [https://en.wikipedia.org/wiki/Yorkshire_Dales] |
| 5   | Hawes           | [https://en.wikipedia.org/wiki/Hawes]           |

## Licence and attribution

Wikipedia text is available under the
[Creative Commons Attribution-ShareAlike 4.0 licence](https://creativecommons.org/licenses/by-sa/4.0/deed.en).
Coordinates are attributed to the Wikipedia contributors of the articles
above. This file is shared under the same license

## What the coordinates mean

A centroid is an approximate centre of the region, deliberately never a real pitch (ADR 001).

## Editing rules

- IDs are permanent and never reused or renumbered.
- Parents are referenced by ID only.
- Types are the strings `Country`, `Area` or `Locality`.
- Maximum depth is three levels (ADR 006).

## Adding a region

1. Use the next free ID.
2. Check the parent exists.
3. Run `dotnet test`.

## Validation

Regions are validated in `Lantern.IntegrationTests/Persistence/RegionSeedTests.cs`, which currently checks for a valid name and valid coordinates.
