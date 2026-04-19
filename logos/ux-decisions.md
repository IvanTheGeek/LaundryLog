---
area: UX Decisions
status: reviewed
---

# LaundryLog UX Decisions

Design decisions that must survive into FnHCI rendering. These emerged from
iterative Claude and Penpot design sessions. They are not optional polish —
they reflect what made the app feel right.

When a future renderer decision conflicts with these, the burden is on the new
decision to justify why it is better.

---

## Batch Entry Is The Main Flow

Do not optimize for a separate single-entry screen. The batch flow handles the
single-entry case naturally. One location context, one repeated-entry surface,
quick re-entry without mode switching.

---

## Location Is The First Context

Location is not secondary to machine or payment controls. It is the first,
top-most context:

- shown prominently at the top of the entry surface
- remains visible and active once set
- stays active until the user explicitly changes it

---

## Controls Must Be Thumb-Friendly

Minimum touch targets: 48–56px height. Generous spacing. This applies to:

- primary action buttons
- machine-type chips
- quantity controls
- payment controls

Laundromats and truck stops are not ideal conditions for precision tapping.

---

## No Custom Numeric Modal

Tapping quantity or unit price uses the device native keypad — not a custom
in-app numeric modal. Layout must allow scroll/repositioning so the field is
not obscured by the keyboard.

---

## Session Total Placement

Session total belongs near the primary command area — under or near the
Log Expense button. Not in a top summary card. Not hidden. The running total
is part of the command surface, not a report.

---

## Visual Direction: Light Orange, Neutral Slate

- Light orange for emphasis (not deep orange — felt too heavy)
- Slate/neutral supporting tones
- Practical and calm, not loud

---

## Labels Integrated, Not Stacked

Reduce or remove heavy section headings where controls carry the meaning.
`Location` is a real heading/context anchor. `Quantity` and `Unit Price` use
direct labels integrated with their controls. The screen should feel compact
and direct.

---

## Unit Price Helpers Have Meaning

Helper values near the price input are meaningful suggestions, not decorative chips:

- Last used during the current session
- Historical at this location
- Community/default fallback

Use "Historical" not "your usual" or similar weak labels.

---

## Payment Is Sticky Within A Session

Payment type is likely the same across all entries in a laundry session. It
should remain easy to change, but the last selection should persist. Card/App
name details appear through progressive disclosure — not always-visible clutter.

---

## Validation Row Is A Command-Readiness Surface

The status/validation row (Location · Type · Payment) is not decorative. It
expresses readiness to log. It should be visible in the main entry surface,
not in a separate report.

---

## Success State After Logging

After a successful log entry:

- Toast: `✓ Entry logged — $5.00 · CASH`
- Button: `✓ Logged!`
- Then: machine type resets; entry added to recent list; form ready for next entry

The form does not navigate away. It stays ready for the next quick entry.

---

## Recent Entry Cards Are Session Context

Recent entries at the bottom of the screen are not leftover list items. They
are the visible session window — compact, easy to scan, clearly tied to the
current 3-hour session. Their presence confirms the session total is correct.

---

## Penpot Reference

The surviving design artifact from the Claude-era design sessions:

- `laundrylog-v7.html` (in CheddarBooks workspace — reference only)
- Penpot board: `Screen.NewSession` at 375×667, `Screen.EntryForm` at 375×1384
- Component: `Button.Counter` for quantity +/−

These are references for FnHCI target shapes, not port targets.
