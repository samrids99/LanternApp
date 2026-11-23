# Development log

A running record of what I did, what went wrong and what I learned. Newest entries at the bottom.

---

## 2026-11-02: Project kickoff

**Done**

- Settled on the idea: a story-first vanlife app, sitting between Park4night (utility) and Instagram (inspiration).
- Chose the stack: React Native (Expo), ASP.NET Core, EF Core and PostgreSQL.
- Checked the name "Lantern": Companies House shows a "same as" match (only relevant if registering a company), and the only notable app store result was Lantern VPN. UK IPO search still to do.
- Wrote the first draft of the project brief.
- Created the GitHub repo and pushed the initial commit.

**Problems and fixes**

- First push failed. My local branch was `master` and there were no commits, because Git doesn't track empty folders. Fixed by renaming the branch to `main`, adding placeholder files and committing before pushing.

**Learned**

- Git ignores empty folders, so `.gitkeep` files are needed.
- Companies House name checks and trademarks are separate things.

---

## 2026-11-03: Planning documents

**Done**

- Moved the brief into `docs/brief.md` and wrote a README that links to it.
- Wrote ADRs 001 to 005: region-only location, working name, tech stack, EXIF stripping, cursor pagination.
- Created GitHub labels, a Project board and 14 user stories as issues (11 MVP, 3 Phase 2).

**Decisions**

- No exact coordinates are collected, ever. Location is a user-chosen region (ADR 001).
- Likes, follows and search moved to Phase 2 to keep the MVP small.

**Learned**

- Acceptance criteria make each story testable, and each issue can later be closed by a pull request using `Closes #n`.

---

## 2026-11-04: Wireframes and region picker

**Done**

- Wireframed the core screens in Figma (feed, stay details, create stay, region picker) and added them to `docs/wireframes/`.
- Worked through the region picker design and wrote ADR 006.

**Problems and fixes**

- Figma wouldn't export my designs because they were loose on a page. Wrapping each screen in a frame fixed it.

**Decisions**

- Region picker is a searchable, hierarchical list rather than a map pin, so the interface itself discourages dropping a pin on the exact spot (ADR 006).

**Next**

- Scaffold the API solution and add CI, then scaffold the Expo app.
- Write the design tokens.
- Start Week 2: auth, region seeding and create-stay.

---

## 2026-11-04: API scaffold and CI

**Done**

- Scaffolded the API solution on .NET 10 with five projects: Domain, Application, Infrastructure, Api and UnitTests. References point inwards (Api → Application → Domain), so business logic doesn't depend on the database or web framework.
- Pinned the SDK with `global.json` and reduced `Program.cs` to a minimal app with OpenAPI and a `/health` endpoint.
- Added a root `.gitignore` covering .NET, Node/Expo and secrets.
- Wrote the first GitHub Actions workflow (`api-ci.yml`): on every push to `main` and every PR touching `api/`, it restores, builds and tests on a clean Ubuntu machine.
- Opened a PR for the scaffold. CI result: [green / failed, and what I fixed].

**Decisions**

- Moved to .NET 10 because .NET 8 support ends this month (ADR 007).
- Used a layered architecture so logic is testable without a database (ADR 003).

**Problems and fixes**

- **242 files staged.** I nearly committed build output because the repo had no root `.gitignore`. `git status` only showed `src/` and `tests/` as collapsed folders, so I only found `bin/` and `obj/` by running `git status -uall`. Fixed by adding a `.gitignore` before committing.
- **`master` vs `main`.** My default branch was `master`, but the workflow's push trigger was `main`, so CI wouldn't have run after merging. The PR trigger would still have worked, which made the problem easy to miss. Fixed by renaming the branch to `main` [or: by changing the trigger].
- **PowerShell vs bash.** Wildcards like `src/*/*.csproj` don't expand in every shell, so I listed projects explicitly.

**Learned**

- CI starts from an empty machine each run, so it catches things that only worked locally.
- The `paths` filter saves runs on unrelated changes, but would block merging if I made the check required and then opened a docs-only PR, because the workflow wouldn't run.
- `--no-restore` and `--no-build` stop later steps repeating earlier work.
- `.gitignore` should exist before the first scaffold, and checking `git status -uall` or `git diff --cached --stat` before committing is worth the few seconds.
- A PR description should say what changed, why and how to test it, not list files.

**Next**

- Scaffold the Expo app and add `mobile-ci.yml`.
- Write the design tokens.
- Start Week 2: ASP.NET Core Identity and JWT auth, then region seeding and create-stay.

---

## 2026-11-17: Seeder integration tests and the region read path

**Done**

- Added Testcontainers integration tests for `RegionSeeder`: a shared
  Postgres fixture applying the real migrations, and tests for insert,
  idempotency, update-on-difference, input order, never-delete and parent
  links.
- `SeedAsync` now returns `RegionSeedResult(Inserted, Updated)` so tests
  assert on counts, not log text.
- Started the read path: `RegionDto` (positional record, `RegionType`
  enum) and `IRegionRepository` with one method, `GetAllAsync`, ordered by
  `Id`.
- Configured `JsonStringEnumConverter` in Api (not yet verified: no
  endpoint).

**Decisions**

- Enum on the DTO, not a string, so the generated TypeScript client gets a
  union of valid values. Cost: the Domain enum is now part of the public
  contract.
- Serializer settings live in Api, not on the Domain enum.
- Repository returns DTOs (reads) and will project in the query. No
  service until there is real logic to put in it.
- Only `GetAllAsync` for now. `GetById` and child lookups wait for a real
  caller.

**Learned**

- Breaking the code on purpose is what proves a test.
- Forcing every row to `Modified` made the idempotency test and the
  changed-value test fail (`Updated = 5`). The exact count caught
  something an end-state assertion would have missed.
- Records, structs and classes differ in copying and equality, and
  `public` versus `internal` decides who may depend on a type.
- Enums serialise as numbers by default, so a string enum has to be
  configured, and that belongs in Api.

**Next**

- `RegionRepository` in Infrastructure (order, project, `AsNoTracking`),
  with a test proving the order.
- Collection fixture so test classes share one container.
- `GET /regions`, then `curl` and `/openapi/v1.json` to check the enum
  appears as names.
- ADRs: enum on the wire, and the ADR 008 edits (sort reasoning, tests now
  exist).
- Proximity test.

## 2026-11-19: First endpoint: GET /regions

**What I did**
- Chose minimal APIs over controllers and wrote ADR 010, including the costs I'm accepting (less built-in structure, I have to keep it tidy myself).
- Built `GET /regions` as a `RegionEndpoints` class with `MapGroup("/regions")` and a handler taking `IRegionRepository` and a `CancellationToken`.
- Checked it with `curl` and the OpenAPI document, then added a test for how far a locality can be from its parent region.

**What went wrong, and what I learned**
- **Route and handler disagreed.** I copied the `/{id:int}` example, so `GET /regions` returned 404 while `GET /regions/3` would have returned every region. The route, handler name, parameters and body all have to describe the same operation. I also now understand that `MapGroup` adds its prefix to each endpoint's path.
- **A 404 on correct-looking code.** Before suspecting the code again: is the running process the new build, and is `MapRegionEndpoints()` actually called? [fill in which it was]
- **`IResult` hides the response type.** The OpenAPI document showed `200 OK` with no content. Switching to `TypedResults.Ok` made the schema appear. `Ok<T>` is the type, `TypedResults` is the factory.
- **My seed tests missed a real data bug.** The curl output showed Hawes under the Peak District instead of the Yorkshire Dales. My tests checked that a parent *exists*, not that it's the *right* one. The new proximity test (haversine, 50 km) failed with "Hawes is 111.16 km away from Peak District", and passed once I fixed the data. The threshold sits between the valid pairs (about 15 km) and the error.
- **`Assert.Fail` and the compiler.** Removing a `return;` after it broke the build: the method throws, but the compiler doesn't know it never returns. Also, `Assert.NotEmpty` inside an `Assert.All` lambda guards nothing, because an empty list never runs the lambda.

**Also from 18 Nov (not logged then)**
- One Postgres container is shared by all test classes with a collection fixture. `ResetAsync` is what keeps tests independent: without it, leftover rows make failures depend on test order.
- Added an `.editorconfig` and ran the formatter as its own commit first, so the feature commits stayed readable. The formatter stripped the byte order mark from the generated EF migration, so `charset = utf-8-bom` for migrations stops that.

**Decisions**
- Minimal APIs in `Lantern.Api/Endpoints/` (ADR 010).
- 18 tests passing, PR merged.

## 2026-11-23: Strict numbers and HTTP-level tests

**What I did**
- Set `JsonNumberHandling.Strict`: the OpenAPI document had typed `id` and the coordinates as number-or-string, and afterwards `id` was a plain `integer`.
- Added `Microsoft.AspNetCore.Mvc.Testing`, a `LanternApiFactory`, and four tests in `Api/RegionEndpointsTests.cs` that parse the raw JSON.
- Wrote ADR 011 (enums as names, strict numbers).

**What went wrong, and what I learned**
- **"Hosting failed to start" after seeding.** The seeder log was fine; a stale terminal was still running the old API on port 5073. `lsof -i :5073` found it.
- **`dotnet add package` took the latest version** (11.0.0) for a net10.0 project and failed with `NU1202`. I now pin a version that matches the target framework, and replace a floating `10.0.*` with the exact one.
- **The factory runs the app as `Testing`, not `Development`.** That way user secrets never load and the dev seeding doesn't run. If the connection string key is wrong, the app fails to start instead of quietly using another database. I use `UseSetting` for the connection string.
- **Parsing raw JSON in the tests** (`JsonDocument`, `ValueKind`) rather than deserialising into `RegionDto`, which would accept either enum shape and hide the thing I'm testing.
- **I proved each test can fail:** removing `JsonStringEnumConverter` broke the enum test, and `WhenWritingNull` broke the null-parent test (a missing property, not a wrong value).
- **A claim I got wrong.** I expected the contract test to guard `NumberHandling.Strict`, but `GET /regions` doesn't read a body and OpenAPI isn't mapped in `Testing`. The line is currently unguarded. A passing suite only tells me about what's actually tested.

**Decisions**
- Enums as names, strict numbers, configured in the Api layer (ADR 011).
- 22 tests passing.

**Still open**
- A test for strict numbers, once an endpoint takes a request body.
- Seed README and licence for the region data; ADR 008 edits; tests for the seeder's missing-parent and parent-loop errors; the placeholder unit test.
