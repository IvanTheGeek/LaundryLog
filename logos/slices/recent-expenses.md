---
slice: RecentExpenses
kind: ViewSlice
concern: Business
path: PATH1
status: Specified
---

# RecentExpenses

**Actor:** Human of Driver
**Intent:** Show the driver a running list of what they have logged during their current
laundry outing, so they can verify entries and track their session total.

## What a session is

Session is a **UI-only concept** — it is not stored. A session is all `LaundryExpenseLogged`
events where no gap between consecutive entries (by `occurredAt`) exceeds 3 hours.

If the driver has not logged an expense in more than 3 hours, the next entry begins a
new session and the list resets to show only that entry. The prior entries are not gone —
they are simply outside the 3-hour window and no longer in scope for this view.

## Given / When / Then

### PATH1 — one entry in session

| Given | When | Then |
|---|---|---|
| `LaundryExpenseLogged { machineType = Washer, quantity = 1, unitPrice = 3.00, lineTotal = 3.00, payment = Cash, occurredAt = T }` | Query at time `T + 30min` (within 3-hour window) | `[ { lineTotal = 3.00, detail = "1 Washer @ $3.00 • Cash", occurredAt = T } ]` |

### Two entries in same session

| Given | When | Then |
|---|---|---|
| `LaundryExpenseLogged { machineType = Washer, quantity = 2, unitPrice = 3.75, lineTotal = 7.50, payment = Card "Business SPARK", occurredAt = T }` | Query at time `T + 65min` (within 3-hour window) | `[ { lineTotal = 7.50, detail = "2 Washers @ $3.75 • Card · Business SPARK", occurredAt = T }, { lineTotal = 5.00, detail = "2 Dryers @ $2.50 • Card · Business SPARK", occurredAt = T+1hr } ]` |
| `LaundryExpenseLogged { machineType = Dryer, quantity = 2, unitPrice = 2.50, lineTotal = 5.00, payment = Card "Business SPARK", occurredAt = T+1hr }` | | |

### Gap exceeds 3 hours — new session

| Given | When | Then |
|---|---|---|
| `LaundryExpenseLogged { ..., occurredAt = T }` | Query at time `T + 4hr` with new entry at `T + 4hr` | `[ { lineTotal = ..., detail = "...", occurredAt = T+4hr } ]` — prior entry outside window, not shown |

## View shape: RecentExpenses

```
sessionTotal : decimal          -- sum of all lineTotals in current session window
entries      : VisibleExpenseLine list  -- ordered by occurredAt descending

VisibleExpenseLine:
  lineTotal   : decimal
  detail      : string          -- formatted detail line (see log-expense.md)
  occurredAt  : DateTimeOffset  -- stored UTC; displayed in local time
```

## Notes

- `occurredAt` is stored UTC; the view layer converts to local time for display.
- The heading on screen is **"TODAY'S ENTRIES"** in PATH1 — may not be accurate past
  midnight on a long session. Acceptable for PATH1; a future PATH may refine the label.
- Session total is the sum of all `lineTotal` values in the current window — displayed
  in the summary bar above the list.
- Entries are shown newest-first (descending `occurredAt`).
