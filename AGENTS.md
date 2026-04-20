# LaundryLog

> Workspace instructions: `../AGENTS.md`, `../FSHARP.md`, `../WORKFLOW.md`
> NEXUS knowledge base: `../NEXUS-LOGOS/`

## Purpose

LaundryLog is a mobile expense tracker for over-the-road truck drivers to document
laundry expenses for IRS tax deduction purposes. It is the primary proof-of-concept
domain for the NEXUS/FORGE methodology.

**Core use case:** Standing in a laundromat parking lot, on a phone, with bad cell
coverage — log what was washed, how much it cost, and how it was paid. Local-first,
offline-capable, IRS-audit-ready.

## Domain

**Each entry is exactly one machine type — never mixed:**
- Machine type: `Washer | Dryer | Supplies`
- Quantity: 1–9 inclusive (Washer/Dryer only; Supplies has no quantity)
  - Entered via stepper buttons (+/−) or by tapping the displayed number to type directly
  - Valid set: {1,2,3,4,5,6,7,8,9} — 0 and 10+ are structurally excluded
- Unit price (Washer/Dryer) or amount (Supplies)
- Line total = quantity × unit price (calculated)
- Payment method: `Cash | Card of name | App of name | Points of name`
- Location (set once per session, carries across entries)
- Timestamp (UTC, assigned by store; displayed in local time)

**Session (UI concept only):** The list of recent expenses shown at the bottom of the
main screen. A session is all expenses where no gap between consecutive entries exceeds
3 hours. Not stored — derived by the RecentExpenses view at read time.

**Payment sub-identity:**
- Cash — no sub-name
- Card — named by driver (e.g. "Business SPARK", "Disney") — free-form in PATH1
- App — named app (e.g. "LaundryPay", "CSC") — free-form in PATH1
- Points — named program (e.g. "Love's Rewards") — free-form in PATH1
- Detail line format: `{qty} {type} @ ${price} • {method}` or `{method} · {name}`

**Supplies expense:** no quantity; just an amount and optional note for IRS documentation.

**Payment source matters downstream:** business-owned card (no expense report needed)
vs. personal card/cash (triggers expense report reimbursement process). Future PATH.

## Architecture

- Client-side PWA / WASM — offline-first; laundromat parking lots have bad cell coverage
- OPFS (Origin Private File System) for durable on-device storage
- Event-sourced — every logged expense is an immutable fact
- Local-first — syncs when connectivity allows
- NEXUS-STRATUM InMemory backend for walking skeleton; OPFS backend to follow

## Logos Structure

```
logos/
  paths/     — PATH records (TOML) — source of all example data
  slices/    — GWT specs per business slice (markdown)
```

## PATHs

| ID | Name | Status |
|---|---|---|
| PATH1 | Fresh Launch — No Location — First Entry | Specified |

## Concern Layers

| Layer | Examples |
|---|---|
| ApplicationLifecycle | AppStarted |
| RuntimeOrchestration | RouteResolved |
| Business | LaundryLocationCaptured, LaundryExpenseLogged, RecentExpenses |
