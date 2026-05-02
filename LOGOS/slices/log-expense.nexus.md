+++
id      = "log-expense"
kind    = "CommandSlice"
command = "LogExpense"
event   = "LaundryExpenseLogged"
concern = "Business"
status  = "Specified"

# Example data scoped to this slice — derived from PATH1 example_data.
# ATLAS uses this to populate the storyboard step for this slice.
[example]
machine_type = "Washer"
quantity     = 1
unit_price   = 3.00
line_total   = 3.00
payment      = "Cash"
payment_name = ""
location     = "Love's #123 - Springfield, OH"
detail_line  = "1 Washer @ $3.00 • Cash"
+++

# LogExpense → LaundryExpenseLogged

**Actor:** Human of Driver
**Intent:** Record a single laundry expense transaction. One entry = one payment event
at a machine (or vending purchase for Supplies). Two machines of the same type at
different prices = two entries.

## Given / When / Then

### Washer / Dryer (PATH1 — single washer, cash)

| Given | When | Then |
|---|---|---|
| `LaundryLocationCaptured { location = "Love's #123 - Springfield, OH" }` | `LogExpense { machineType = Washer, quantity = 1, unitPrice = 3.00, payment = Cash }` | `LaundryExpenseLogged { location = "Love's #123 - Springfield, OH", machineType = Washer, quantity = 1, unitPrice = 3.00, lineTotal = 3.00, payment = Cash }` |

### Washer / Dryer — multiple machines, named card

| Given | When | Then |
|---|---|---|
| `LaundryLocationCaptured { location = "Love's #123 - Springfield, OH" }` | `LogExpense { machineType = Washer, quantity = 2, unitPrice = 3.75, payment = Card "Business SPARK" }` | `LaundryExpenseLogged { ..., quantity = 2, unitPrice = 3.75, lineTotal = 7.50, payment = Card "Business SPARK" }` |

### Supplies — amount only, no quantity

| Given | When | Then |
|---|---|---|
| `LaundryLocationCaptured { location = "Love's #123 - Springfield, OH" }` | `LogExpense { machineType = Supplies, amount = 2.50, note = Some "bleach packet", payment = Cash }` | `LaundryExpenseLogged { ..., machineType = Supplies, amount = 2.50, note = Some "bleach packet", payment = Cash }` |

## Event: LaundryExpenseLogged

| Field | Type | Value (PATH1) | Notes |
|---|---|---|---|
| `location` | `string` | `"Love's #123 - Springfield, OH"` | Carried from ambient LocationCaptured |
| `machineType` | `MachineType` | `Washer` | `Washer \| Dryer \| Supplies` |
| `quantity` | `int option` | `Some 1` | `None` for Supplies |
| `unitPrice` | `decimal option` | `Some 3.00` | `None` for Supplies |
| `lineTotal` | `decimal` | `3.00` | `quantity × unitPrice` for machines; `amount` for Supplies |
| `payment` | `PaymentMethod` | `Cash` | `Cash \| Card of string \| App of string \| Points of string` |
| `note` | `string option` | `None` | Supplies only; IRS documentation |
| `occurredAt` | `DateTimeOffset` | _(assigned by store, UTC)_ | Display in local time |

## Payment display

| Method | Detail line format |
|---|---|
| Cash | `1 Washer @ $3.00 • Cash` |
| Card | `1 Washer @ $3.75 • Card · Business SPARK` |
| App | `1 Washer @ $3.00 • App · LaundryPay` |
| Points | `1 Dryer @ $2.50 • Points · Love's Rewards` |

## Notes

- `lineTotal` is always stored — not recalculated at read time.
- `Log Expense` button is disabled until machine type and payment method are both selected.
- Card, App, and Points names are free-form text in PATH1. Named lists are a future PATH.
- Payment source (business-owned vs. personal) is implicit in the card name for now.
  Downstream expense report workflow is a future PATH.
