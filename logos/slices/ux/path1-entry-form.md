---
slice: PATH1 — Entry Form
kind: UX
concern: UX
path: PATH1
status: Specified
---

# PATH1 — Entry Form UX Spec

This is the UX-domain lens on PATH1. The business-domain spec (SetLocation,
LogExpense, RecentExpenses) lives in `logos/slices/`. This spec defines the
interaction model: what the driver does tap-by-tap, what commands the UI
dispatches, and what state the form maintains at every step.

This is the Spec that FORGE consumes to generate the entry form components.

---

## UX Types

### PaymentKind (UX-only — no sub-name)

```fsharp
type PaymentKind = Cash | Card | App | Points
```

Distinct from the business `PaymentMethod` (`Cash | Card of string | ...`).
The form holds kind and detail separately; they combine into `PaymentMethod`
only at submit time.

---

## UX Commands

Commands the entry form dispatches. The parent (AppComponent / walking-skeleton
event store) handles each command, updates the View, and re-renders.

### Location Screen

| Command | Payload | Trigger |
|---|---|---|
| `LocationTextChanged` | `string` | Every keystroke in the location text field |

`SetLocation` is a **business command** — dispatched when the driver taps
the confirm button, not a UX command. The UX domain observes the precondition
(`canConfirm`) but does not own the event.

### Entry Form

| Command | Payload | Trigger |
|---|---|---|
| `MachineTypeSelected` | `MachineType` | Washer / Dryer / Supplies chip tapped |
| `QuantityChanged` | `int` | Stepper Increment, Decrement, or DirectEntry resolved |
| `UnitPriceChanged` | `decimal option` | Price field input accepted; `None` if cleared |
| `AmountChanged` | `decimal option` | Supplies amount field; `None` if cleared |
| `NoteChanged` | `string option` | Supplies optional note field |
| `PaymentKindSelected` | `PaymentKind` | Cash / Card / App / Points chip tapped |
| `PaymentDetailChanged` | `string` | Sub-name field for Card / App / Points |
| `SubmitTapped` | _(none)_ | Log Expense button — only fires when `canSubmit = true` |

---

## Views

### LocationFormView

```
{
  text:       string   -- current field content
  canConfirm: bool     -- text.Length > 0
}
```

### EntryFormView

```
{
  -- Ambient context (displayed, not editable here)
  location: string

  -- Machine type — drives form shape
  machineType: MachineType option   -- None until driver selects

  -- Washer / Dryer fields (shown when machineType = Washer | Dryer)
  quantity:  int            -- default 1; valid range 1–9
  unitPrice: decimal option -- None until valid entry

  -- Supplies fields (shown when machineType = Supplies)
  amount: decimal option    -- None until valid entry
  note:   string option     -- optional IRS documentation note

  -- Payment — kind drives sub-name field visibility
  paymentKind:   PaymentKind option -- None until driver selects
  paymentDetail: string             -- sub-name for Card/App/Points; empty for Cash

  -- Derived
  lineTotal:  decimal option  -- quantity × unitPrice (machines); amount (Supplies); None if incomplete
  canSubmit:  bool            -- see logic below

  -- Post-submit feedback
  lastLogged: LoggedSummary option  -- drives success toast; None until first submit
}
```

### LoggedSummary

```
{
  message: string   -- e.g. "✓ Entry logged — $3.00 · CASH"
}
```

---

## canSubmit Logic

All conditions must hold:

```
machineType is Some
AND (
     machineType ∈ { Washer, Dryer } AND unitPrice is Some AND unitPrice > 0
  OR machineType = Supplies          AND amount    is Some AND amount    > 0
)
AND paymentKind is Some
AND (paymentKind = Cash OR paymentDetail.Length > 0)
```

The Log Expense button is **structurally disabled** (not just styled) when
`canSubmit = false`. `SubmitTapped` is never dispatched from a disabled button.

---

## GWT — UX Slices

### MachineTypeSelected

| Given | When | Then |
|---|---|---|
| `{ machineType = None, quantity = 1, unitPrice = None, canSubmit = false }` | `MachineTypeSelected Washer` | `{ machineType = Some Washer, quantity = 1, unitPrice = None, canSubmit = false }` — stepper and unit price shown; amount hidden |
| `{ machineType = Some Washer, quantity = 2, unitPrice = Some 3.75, canSubmit = false (no payment) }` | `MachineTypeSelected Supplies` | `{ machineType = Some Supplies, quantity = 1 (reset), unitPrice = None (cleared), amount = None, canSubmit = false }` — quantity and unit price hidden; amount and note shown |
| `{ machineType = Some Supplies, amount = Some 2.50, canSubmit = false (no payment) }` | `MachineTypeSelected Dryer` | `{ machineType = Some Dryer, quantity = 1 (reset), amount = None (cleared), unitPrice = None, canSubmit = false }` — amount hidden; stepper and unit price shown |

Switching machine type clears the price/amount fields and resets quantity.
A driver who switches from Washer to Dryer mid-entry is starting fresh on
the quantity/price — keeping stale Washer values would be misleading.

### PaymentKindSelected

| Given | When | Then |
|---|---|---|
| `{ paymentKind = None, paymentDetail = "" }` | `PaymentKindSelected Cash` | `{ paymentKind = Some Cash, paymentDetail = "" }` — no sub-name field |
| `{ paymentKind = None, paymentDetail = "" }` | `PaymentKindSelected Card` | `{ paymentKind = Some Card, paymentDetail = "" }` — sub-name field shown; canSubmit still false (detail empty) |
| `{ paymentKind = Some Card, paymentDetail = "Business SPARK" }` | `PaymentKindSelected Cash` | `{ paymentKind = Some Cash, paymentDetail = "" (cleared) }` — sub-name hidden and cleared; switching to Cash removes the name |
| `{ paymentKind = Some Card, paymentDetail = "Business SPARK" }` | `PaymentKindSelected App` | `{ paymentKind = Some App, paymentDetail = "" (cleared) }` — switching between named kinds clears the detail; the Card name is not a valid App name |

### UnitPriceChanged

| Given | When | Then |
|---|---|---|
| `{ machineType = Some Washer, quantity = 1, unitPrice = None, paymentKind = Some Cash, canSubmit = false }` | `UnitPriceChanged (Some 3.00)` | `{ unitPrice = Some 3.00, lineTotal = Some 3.00, canSubmit = true }` |
| `{ unitPrice = Some 3.00, quantity = 2, lineTotal = Some 6.00, canSubmit = true }` | `UnitPriceChanged None` | `{ unitPrice = None, lineTotal = None, canSubmit = false }` |

### SubmitTapped — PATH1 example (Washer, Cash)

| Given | When | Then |
|---|---|---|
| `{ location = "Love's #123 - Springfield, OH", machineType = Some Washer, quantity = 1, unitPrice = Some 3.00, paymentKind = Some Cash, paymentDetail = "", canSubmit = true }` | `SubmitTapped` | Business `LogExpense { machineType = Washer, quantity = 1, unitPrice = 3.00, payment = Cash }` dispatched; form resets per policy below; `lastLogged = Some { message = "✓ Entry logged — $3.00 · CASH" }` |

---

## Form Reset Policy

Applied immediately after a successful `LogExpense` event is confirmed.

| Field | After submit |
|---|---|
| `machineType` | `None` — driver must re-select for next entry |
| `quantity` | `1` — default |
| `unitPrice` | `None` |
| `amount` | `None` |
| `note` | `None` |
| `paymentKind` | **Sticky** — keeps last selection |
| `paymentDetail` | **Sticky** — keeps last sub-name |
| `lastLogged` | Set to summary for toast |

Rationale: machine type varies per machine. Price varies per machine. Payment
is the same across a laundry session — sticky reduces re-entry friction.

---

## Form Shape by Machine Type

| Field / Control | Washer | Dryer | Supplies |
|---|---|---|---|
| Machine type chips | ✓ | ✓ | ✓ |
| Quantity stepper | ✓ | ✓ | — |
| Unit price input | ✓ | ✓ | — |
| Amount input | — | — | ✓ |
| Note field | — | — | ✓ (optional) |
| Payment chips | ✓ | ✓ | ✓ |
| Payment detail field | if Card/App/Points | if Card/App/Points | if Card/App/Points |
| Line total display | ✓ | ✓ | ✓ |
| Log Expense button | ✓ | ✓ | ✓ |

---

## Payment Method → Business Type Mapping

At submit time, `paymentKind` + `paymentDetail` combine into the business `PaymentMethod`:

| `paymentKind` | `paymentDetail` | Business `PaymentMethod` |
|---|---|---|
| `Cash` | `""` | `Cash` |
| `Card` | `"Business SPARK"` | `Card "Business SPARK"` |
| `App` | `"LaundryPay"` | `App "LaundryPay"` |
| `Points` | `"Love's Rewards"` | `Points "Love's Rewards"` |

---

## Components → Commands / View Fields

The mapping from components to what they read and dispatch. This is the
interface contract FORGE uses to generate each component.

| Component | Reads from View | Dispatches |
|---|---|---|
| `LocationInput` | `text`, `canConfirm` | `LocationTextChanged` |
| `MachineTypeChips` | `machineType` | `MachineTypeSelected` |
| `Stepper` | `quantity`, `Min=1`, `Max=Some 9` | `QuantityChanged` (via `StepperCommand`) |
| `MoneyInput` (unit price) | `unitPrice` | `UnitPriceChanged` |
| `MoneyInput` (amount) | `amount` | `AmountChanged` |
| `NoteInput` | `note` | `NoteChanged` |
| `PaymentChips` | `paymentKind` | `PaymentKindSelected` |
| `PaymentDetailInput` | `paymentDetail`, `paymentKind` (for placeholder) | `PaymentDetailChanged` |
| `LineTotalDisplay` | `lineTotal` | _(read-only)_ |
| `SubmitButton` | `canSubmit`, `lastLogged` | `SubmitTapped` |
