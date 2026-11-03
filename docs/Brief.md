Lantern

Lantern: Project Brief

Working title. Draft v0.1. Author: [your name]. Date: 2 November 2026.

1. Overview

Lantern is a mobile app where vanlifers share the places they loved staying and the stories that went with them. It sits between utility apps like Park4night and inspiration platforms like Instagram. It is story-first: not just where to park, but what the stay felt like, what was eaten, where the good pub was, and which walks started from the door.

It is also my main portfolio project, built to demonstrate the skills I am developing on my Level 4 Software Developer apprenticeship.

2. Problem

Utility apps (e.g. Park4night) are excellent for logistics but cold. They answer “can I park here?” and not “what will it be like?”

Social platforms (e.g. Instagram) are good for inspiration but poor for places. Posts are hard to search by area, and the practical detail is lost.

Newcomers to vanlife, myself included, often feel uncertain about arriving somewhere unfamiliar. Other people’s real experiences make the unknown feel less daunting.

Popular wild spots can suffer when exact locations are shared widely, through overcrowding and landowner conflict.

3. Target users

Primary persona: the prospective or new vanlifer

Currently converting or newly on the road (I am one, midway through my own conversion)

Phone-first, often with patchy signal

Values community and atmosphere over pure logistics

Wants to find inspiration for an area and plan the experience around it

Respects the places they stay and wants to protect them

Secondary persona: the experienced vanlifer

Wants to document trips and share favourite stays

Wants a personal travel log that is more meaningful than camera roll photos

4. Core loop

A user posts a stay with photos, a story and a region.

It appears in the feed as a warm, visual card.

Tapping the card opens the stay details page: what they ate, where to eat and drink locally, walks nearby, and practical notes.

Each stay adds to the user’s travel log.

The region map shows where the community has stayed.

5. Why “Lantern”

A lantern does two jobs, and the app has two halves that match them.

Warmth: the cosy inside of the van, the stories, the suppers, the local pub.

Light: a small, gentle light on unfamiliar places, so they feel less like the unknown.

A lantern lights a small circle and not the whole landscape. That mirrors the privacy principle: the app shows an area and leaves the exact spot to be discovered.

Status: working title. Checked so far: Companies House (a “same as” match exists, only relevant if registering a company) and app stores (Lantern VPN found, a different product). UK IPO trademark search still to do. See ADR 002.

6. MVP scope

In scope

Sign up, log in and basic profile

Create, edit and delete a stay (title, story, region, arrival date, nights, photos)

Structured recommendations on a stay: food, pub, walk, other

Feed with cursor-based pagination

Stay details page

Personal travel log

Region map showing stay counts per region

Practical tags (e.g. coastal, dog-friendly, quiet)

Report button and community guidelines page

Out of scope for the MVP

Exact coordinates or pins (by design, never)

Safety ratings and detailed logistics (this is Park4night’s strength)

Likes, comments and follows (Phase 2)

Trip grouping, saved spots and search filters (Phase 2)

Offline support, notifications and route planning (later)

7. Design principles

Protect the spots. Region-only location. No coordinates are collected or stored. Photo EXIF data is stripped server-side.

Warm and cosy by design. Cream, terracotta, amber and deep forest or night tones, with a soft serif for headings. “Dark” on the map means unexplored, never dangerous.

Mobile-first. Designed for a phone, often on poor signal.

Accessible. WCAG AA contrast, readable text on the dim map, and meaning never conveyed by glow or colour alone.

Gentle by default. Tone of voice is soft and unhurried, with no fear-based wording.

8. Technical approach

API

ASP.NET Core Web API (C#), layered or clean architecture

Auth

ASP.NET Core Identity with JWT access and refresh tokens

CI/CD

GitHub Actions for API and mobile; EAS for mobile builds

Data

EF Core with PostgreSQL

Images

Cloud blob storage, server-side processing and EXIF stripping

Mobile

React Native (Expo), TypeScript, TanStack Query, Zod

Testing

xUnit, NSubstitute, WebApplicationFactory and Testcontainers; Jest and React Native Testing Library; Maestro for end-to-end

Validation

FluentValidation (API), Zod (mobile)

One monorepo holds /api, /mobile and /docs, with one README.

9. Success criteria

The MVP is done when:

A deployed API and a mobile build are available

I can sign up, post a stay with photos, and see it in the feed and in my log

The region map shows stay counts per region

Only the owner can edit or delete their stay (covered by tests)

Uploaded photos have EXIF data stripped (covered by tests)

CI runs build, lint and tests on every push

The README, architecture overview and ADRs are written

The work is traceable through issues, pull requests and a development log

10. Risks and open questions

Risk or question

Mitigation or next step

Region granularity (too broad is useless, too narrow defeats privacy)

Decide in ADR; start with national park, island or coastal stretch level

Recommendations or story text may reveal the spot

Gentle prompts on the form; report and edit route; ADR on recommendation detail

Region data sourcing and seeding

Use open data (OpenStreetMap, Natural Earth, ONS) and check licences

Content moderation

Report button and guidelines for MVP; deeper tools later

Name and trademark

UK IPO search before any public launch

Scope creep

Stick to the MVP list; new ideas go to the Phase 2 backlog

Legality of overnight parking varies by place

Short disclaimer and a “check local rules” prompt

11. Plan

Week 1: Brief, user stories, wireframes, design tokens, repo and CI

Weeks 2–3: API foundation: auth, region seeding, stay CRUD, image upload

Week 4: Mobile auth, feed and stay details

Week 5: Create-stay flow, region map, travel log

Week 6: Deployment, README, ADRs and polish
