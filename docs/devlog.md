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
