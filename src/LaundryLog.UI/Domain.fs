namespace LaundryLog.UI

type MachineType = Washer | Dryer | Supplies

// UX-layer kind — holds kind and detail separately until submit
type PaymentKind = Cash | Card | App | Points

// Business-layer payment — combines kind + sub-name at submit time
type PaymentMethod =
    | CashPayment
    | CardPayment   of name: string
    | AppPayment    of name: string
    | PointsPayment of name: string

/// Flat record stored as JSON in the STRATUM event Data field.
/// No DUs or options — serializes cleanly with System.Text.Json.
/// Quantity = 0 and UnitPrice = 0 for Supplies entries.
type LaundryExpenseData = {
    Location    : string
    MachineType : string   // "Washer" | "Dryer" | "Supplies"
    Quantity    : int
    UnitPrice   : decimal
    LineTotal   : decimal
    Payment     : string   // "Cash" | "Card" | "App" | "Points"
    PaymentName : string   // "" for Cash
}
