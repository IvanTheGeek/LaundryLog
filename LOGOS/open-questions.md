---
area: Open Questions
status: reviewed
---

# LaundryLog Open Questions

Unresolved modeling decisions. Keep these visible. Each one is a fork that will
need to be resolved when the PATH that depends on it is being specified.

---

## Entry Identity

Should `LaundryExpenseLogged` carry a stable entry ID?

Current decision: implicit identity (no ID field) — acceptable until correction
and removal paths appear. When `LaundryExpenseCorrected` or `LaundryExpenseRemoved`
are modeled, a stable entry reference becomes necessary.

---

## Time Semantics: One Timestamp or Two?

`LaundryExpenseLogged` could carry:

- `occurredAt` — when the expense happened (user-entered or device time)
- `recordedAt` — when the entry was created in the app (always system UTC)

These differ when the driver enters an expense retroactively. Current PATH1
uses one timestamp (the store assigns `OccurredAt` at write time). If retroactive
entry is a real use case, two timestamps may be needed earlier than expected.

---

## Location Persistence: User vs. GPS

Should GPS activity be allowed to silently replace the active location?

Current decision: no. GPS should suggest or refine, not silently overwrite.
The active location stays until the user changes it.

---

## Location Acquisition Variants

For future GPS/lookup paths, should `LaundryLocationCaptured` carry:

- text only (PATH1)
- text + optional coordinates
- text + coordinates + matched location identity

Current leaning: same event name, richer payload for later paths. The
`capture_method` field (`manual-text`, `gps-assisted`, `matched-location`)
distinguishes them.

---

## Session Boundary: Derived vs. Explicit

Should session ever become a stored event?

Current decision: derived — the 3-hour gap rule in the RecentExpenses view
is sufficient. Revisit only if audit or IRS reporting requires an explicit
session-open / session-close boundary.

---

## Payment Detail Depth for PATH1

In PATH1, Card/App/Points carry a free-form name. Future PATHs may introduce:

- named card registry (select from known cards)
- business vs. personal categorization at capture time
- automatic categorization from card name patterns

Current decision: free-form name, category implicit in the name. Downstream
expense-report workflow is a future PATH.

---

## Location Name Character Policy

LocationName uses an explicit allowlist: ASCII letters, digits, space, hyphen,
apostrophe, dot, comma, ampersand, hash.

Open question: is this allowlist complete for real location names (e.g.,
international characters in city names, accented characters)?

Current decision: ASCII-only for PATH1. Broaden in a future PATH when a real
failing case appears.

---

## Supplies Display Format

The detail line format for Supplies entries is not fully specified. Machines use
`{qty} {type} @ ${price} • {payment}`. Supplies have no quantity or unit price —
only a total amount and an optional note.

Current placeholder: `Supplies ${amount} • {payment}`. Revisit when a Supplies
PATH is specified.

---

## Multi-Device Convergence Seam

Git was explored as a plausible convergence seam (event log as append-only file
in a Git repo). Not committed to. OPFS covers the single-device offline case.
Multi-device convergence remains deferred and open.
