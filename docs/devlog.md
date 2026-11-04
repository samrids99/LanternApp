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
