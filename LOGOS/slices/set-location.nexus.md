+++
id      = "set-location"
kind    = "CommandSlice"
command = "SetLocation"
event   = "LaundryLocationCaptured"
concern = "Business"
status  = "Specified"

[example]
location = "Love's #123 - Springfield, OH"
+++

# SetLocation → LaundryLocationCaptured

**Actor:** Human of Driver
**Intent:** Record the laundromat location before logging expenses. Location carries
across all expense entries until the app is closed or a new location is set.

**PATH1 note:** Location is free-form text in this path. Structured location capture
(GPS auto-fill, known-location lookup) is addressed in future PATHs.

## Given / When / Then

| Given | When | Then |
|---|---|---|
| _(no prior events — fresh install)_ | `SetLocation { location = "Love's #123 - Springfield, OH" }` | `LaundryLocationCaptured { location = "Love's #123 - Springfield, OH" }` |

## Event: LaundryLocationCaptured

| Field | Type | Value (PATH1) | Notes |
|---|---|---|---|
| `location` | `string` | `"Love's #123 - Springfield, OH"` | Free-form text in PATH1 |
| `occurredAt` | `DateTimeOffset` | _(assigned by store, UTC)_ | Display in local time |

## Notes

- Location is not validated beyond being non-empty in PATH1.
- The Set Location button is disabled until the driver has entered at least one character.
- Location persists as ambient context for all subsequent `LogExpense` commands in the session.
