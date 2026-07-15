# TMS API Versioning Policy

## 1. What counts as a breaking change

Any of the following requires a new API version — it cannot ship into an existing version:

- Removing a field from a response
- Renaming a field in a response or request
- Changing a field's data type (e.g., string → number)
- Changing a status code returned for an existing scenario
- Tightening validation on an existing field (making an optional field required, narrowing an accepted range/format)
- Changing the default sort order of a list endpoint
- Changing the meaning of an existing field without changing its name

If an existing client's code would need to change to keep working, it's breaking.

## 2. What counts as additive (non-breaking)

These are safe to ship into the current version without bumping it:

- Adding a new optional field to a response
- Adding a new endpoint
- Adding a new optional query parameter with a sensible default
- Loosening validation (making a required field optional)
- Adding a new enum value, as long as clients are expected to handle unknown values gracefully

## 3. Sunset window

Once a new major version ships, the previous version stays live for a **minimum of 6 months** before shutdown. This gives clients on slower release cycles (e.g., quarterly-maintained deployments) a full migration window without emergency pressure.

## 4. Communication

From day one of a new version shipping:

- The outgoing version immediately returns `Deprecation`, `Sunset`, and `Link` headers on every response (see `V1DeprecationMiddleware`)
- A `CHANGELOG.md` entry is added describing what changed and why
- Every team holding an API key is notified directly (email/Slack), not just via docs
- The shutdown date is placed on a shared calendar with advance reminders

## 5. Skipping versions

Clients are not required to migrate through every intermediate version. A client on V1 may jump directly to V3 once V3 exists — there is no obligation to integrate with V2 first. Each version's deprecation headers point to its immediate successor, but the final destination is always the latest stable version.

## 6. Header-based versioning (opt-in)

Partners may optionally specify `X-Api-Version` as a header instead of using
the URL segment. URL-segment versioning remains the primary, default
mechanism — it's more visible during incident response (visible in logs,
browser tabs, and curl output without extra inspection). Header-based
versioning exists only as a migration aid for clients with cached/hardcoded
unversioned URLs.
---
*Owner: TMS API team. Review this document whenever a version is added or sunset.*