---
area: Deferred Slices
status: reviewed
---

# Deferred Events and Slices

Events and commands that have been identified but are not needed until PATH1 is
solid. Do not force these into PATH1. They expand from the first path once the
core loop (location → expense → view) is proven.

---

## Soon-After — First Expansion Candidates

These are the most plausible next events after PATH1 is verified:

### `LaundryExpenseCorrected`

Correct an entry without pretending the original never happened. Append-only
correction — does not remove the original `LaundryExpenseLogged`.

Likely fields: original event reference, corrected fields, reason, occurredAt.

### `LaundryExpenseRemoved`

Explicit removal/reversal of an entry. Append-only — does not delete the
original event. The view derives the effective state.

### `LaundryLocationRefined`

Improve a rough initial location capture without erasing the earlier record.
Useful when GPS or lookup becomes available after a manual-text capture.

---

## Deferred — Later Workflows

These require separate modeling passes. Do not spec until the workflow that
needs them is being pathed.

| Event | When It Matters |
|---|---|
| `PaymentStatementLineMatched` | Matching logged expenses to card/bank statement lines |
| `ReceiptEvidenceAttached` | Attaching a photo or file as receipt evidence |
| `LocationSuggestedFromDevice` | GPS providing a candidate location to confirm |
| `ExistingLocationMatchedFromCoordinates` | Matching GPS coordinates to a known location record |
| `LaundrySessionMergedAcrossDevices` | Convergence when the same session was captured on multiple devices |
| `TaxReportEntryPrepared` | Generating IRS-compliant documentation from a date range of entries |

---

## Session Events — Currently Derived

These were considered and deliberately kept as derived view logic rather than
stored events. Revisit only if audit or reporting requirements demand an
explicit stored boundary.

| Event | Current decision |
|---|---|
| `LaundrySessionStarted` | Derived from first entry; 3-hour gap rule in RecentExpenses view |
| `LaundrySessionLocationSet` | Replaced by `LaundryLocationCaptured` — location belongs to the event stream, not a session concept |
| `LaundrySessionEnded` | Derived from inactivity; no explicit close-out needed yet |

---

## Deferred UI Features

UI components visible in the design system that require data not yet available.

### Quick-fill Price Chips (`feature/quickfill-chips`)

Amber-tinted suggestion row below MoneyInput. Three chips: Last used, Historical,
Community. Tapping any chip fires `UnitPriceChanged (Some v)` — same as manual entry.

**Blocked on:**
- STRATUM event store connected (`feature/event-wiring`)
- A price-history projection reading past `LaundryExpenseLogged` events
- Community chip requires an external backend not yet designed

PATH1 has no prior history so the row must not render. Full spec in
`logos/slices/ux/path1-entry-form.md § Deferred UI Features`.

---

## Open Questions That Feed These Slices

See `logos/open-questions.md` for the unresolved decisions that bear on
correction, identity, and time semantics.
