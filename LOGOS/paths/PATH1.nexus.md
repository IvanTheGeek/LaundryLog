+++
id     = "PATH1"
name   = "Fresh Launch — No Location — First Entry"
status = "Specified"
actor  = "Human of Driver"
goal   = "Log the first laundry expense on a fresh install with no prior data"

concern_layers = [
  "ApplicationLifecycle",
  "RuntimeOrchestration",
  "Business",
  "UX",
]

# Canonical example data for this PATH.
# All downstream GWT cases derive from this block — do not invent data elsewhere.
[example_data]
location     = "Love's #123 - Springfield, OH"
machine_type = "Washer"
quantity     = 1
unit_price   = 3.00
line_total   = 3.00
payment      = "Cash"
detail_line  = "1 Washer @ $3.00 • Cash"

# Ordered slice sequence.
# `spec` links to the slice's own *.nexus.md for graph-traversal by ATLAS.
# Slices without `spec` are infrastructure events — no GWT, no handler.

[[slices]]
order   = 1
id      = "app-started"
kind    = "Event"
concern = "ApplicationLifecycle"
note    = "Driver taps app icon. Splash screen appears immediately. No runtime checks complete yet."

[[slices]]
order   = 2
id      = "route-resolved"
kind    = "Event"
concern = "RuntimeOrchestration"
route   = "NeedLocation"
note    = "Runtime checks complete. No prior session, no saved location, no pending draft. Route resolves to NeedLocation screen."

[[slices]]
order   = 3
id      = "set-location"
kind    = "CommandSlice"
concern = "Business"
spec    = "logos/slices/set-location.nexus.md"

[[slices]]
order   = 4
id      = "log-expense"
kind    = "CommandSlice"
concern = "Business"
spec    = "logos/slices/log-expense.nexus.md"

[[slices]]
order   = 5
id      = "recent-expenses"
kind    = "ViewSlice"
concern = "Business"
spec    = "logos/slices/recent-expenses.nexus.md"
+++

# PATH1 — Fresh Launch — No Location — First Entry

A driver opens LaundryLog for the first time. No saved location, no prior events,
no pending draft. The runtime routes to the location capture screen. After setting
a location, the driver logs one washer expense paid with cash.

This is the minimal successful journey.

## Why this PATH

PATH1 establishes the baseline. Every subsequent PATH either starts where PATH1
ends or is a variation of one of its steps. If PATH1 is wrong, the foundation is
wrong. Getting it right means the model is sound for the first-use experience —
the moment that determines whether the app earns ongoing use.

## Actor perspective

The driver is standing in a laundromat parking lot. Cell coverage may be weak.
The app should need zero seconds of thought to operate. Location is entered once
and carried silently across every entry until explicitly changed.

## Design decisions embedded in this PATH

**Location is ambient, not a LogExpense field.** `LaundryLocationCaptured` is a
separate prior event. The projector carries it forward into every expense. This
means the driver never has to type the location again mid-session.

**Session is a view concept, not stored.** The 3-hour gap rule is evaluated by the
RecentExpenses projector at read time. The event store holds no session record. This
keeps events clean and makes replay safe across device restarts.

**Payment is sticky after submit.** Machine type and price reset (they vary per
machine). Payment kind and sub-name persist (they are constant across a laundry
outing). This reduces tap count for multi-machine sessions.

**Location is free-form text in PATH1.** GPS auto-fill, known-location lookup, and
named-location history are future PATHs. Scoping to free-form keeps the domain
minimal and lets the model stabilize before adding input complexity.

## Lifecycle

| Stage | Status |
|---|---|
| Identified | ✓ |
| Modeled | ✓ |
| Specified | ✓ |
| Implemented | ✓ (walking skeleton — InMemory STRATUM) |
| Verified | — |
