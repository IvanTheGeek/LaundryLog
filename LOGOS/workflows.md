---
area: Workflows
status: reviewed
---

# LaundryLog Workflows

The real-world shape of the activity:

1. Arrive at a location
2. Do laundry over time
3. Log expenses as they happen
4. Retain enough context for later recordkeeping

These workflows are the source of future PATHs. PATH1 covers Workflow 1 + 2 (first entry).

---

## Workflow 1 — Capture Location

1. Open app
2. Enter location manually (free-form text in PATH1)
3. Log Entry button disabled until location present
4. Confirm location
5. Location is now the durable active context

GPS lookup and location matching are deferred. Once set, location remains active until the user changes it.

**PATH1 status:** Specified (SetLocation slice).

---

## Workflow 2 — Log Washer Then Dryer

1. Choose machine type (Washer)
2. Enter quantity
3. Enter unit price
4. Choose payment method
5. Review calculated line total
6. Tap Log Expense → entry recorded
7. Repeat for Dryer (same session, same location)

**PATH1 status:** Specified (LogExpense slice, Washer only).
**Future:** Dryer entry is an identical second LogExpense. No new slice needed — same handler, different machine type.

---

## Workflow 3 — Repeat Entry During One Laundry Session

1. Set location once
2. Log first expense
3. Remain in the same location/session context
4. Log additional expenses over time
5. Session total accumulates in the view

Session is a UI concept derived from time gaps between entries. Not stored. The 3-hour gap rule applies.

**PATH1 status:** RecentExpenses slice covers single-entry case.
**Future:** Multi-entry session PATH needed; tests for session-boundary behavior exist in RecentExpenses tests.

---

## Workflow 4 — Batch Entry As The Primary Mode

Batch-oriented flow also handles the single-entry case:

- Quantity defaults to 1
- Same entry form used once or many times
- No separate single-entry mode needed

**PATH1 status:** Covered implicitly — entry form does not distinguish batch from single.

---

## Workflow 5 — Offline Use

- Phone offline at the laundromat or truck stop
- Log expenses locally with no server contact
- Later reconcile across devices when connectivity returns
- No requirement for cloud just to record an expense

**PATH1 status:** InMemory store satisfies PATH1. OPFS backend needed for production.

---

## Workflow 6 — Optional Hosted Assistance

- Local use stands on its own
- Hosted services (location hints, sync) can enrich the experience
- Not required for the app to be functional

**PATH status:** Deferred beyond PATH1.

---

## Deferred Path Vocabulary (from CheddarBooks)

Earlier work used a finer-grained path vocabulary that may inform future PATH splits:

| Slug | Meaning |
|---|---|
| `path1-1-new-session` | App opened, no location yet |
| `path1-2-location-entered` | Location typed, not yet confirmed |
| `path1-3-entry-form` | Location confirmed, entry form active |

PATH1 in the current model covers all three stages as a single path. Future PATHs may split these if runtime branching warrants it.
