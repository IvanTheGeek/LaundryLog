namespace LaundryLog

open System

type MachineType =
    | Washer
    | Dryer
    | Supplies

type PaymentMethod =
    | Cash
    | Card   of name: string
    | App    of name: string
    | Points of name: string

// ─── Command data ─────────────────────────────────────────────────────────────

type SetLocationCommand = {
    Location : string
}

type LogExpenseCommand = {
    MachineType : MachineType
    Quantity    : int option      // None for Supplies
    UnitPrice   : decimal option  // None for Supplies
    Amount      : decimal option  // Supplies only
    Payment     : PaymentMethod
    Note        : string option   // Supplies only — IRS documentation
}

// ─── Event data ───────────────────────────────────────────────────────────────

type LaundryLocationCaptured = {
    Location : string
}

type LaundryExpenseLogged = {
    Location    : string
    MachineType : MachineType
    Quantity    : int option
    UnitPrice   : decimal option
    LineTotal   : decimal
    Payment     : PaymentMethod
    Note        : string option
}

// ─── View data ────────────────────────────────────────────────────────────────

type VisibleExpenseLine = {
    LineTotal  : decimal
    Detail     : string
    OccurredAt : DateTimeOffset
}

type RecentExpensesView = {
    SessionTotal : decimal
    Entries      : VisibleExpenseLine list
}

type RecentExpensesCriteria = {
    QueryTime : DateTimeOffset
}
