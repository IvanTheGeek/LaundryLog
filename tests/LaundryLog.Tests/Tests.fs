module LaundryLog.Tests.Tests

open System
open Expecto
open Nexus.Modeling
open Nexus.Modeling.Testing.Assert
open LaundryLog
open LaundryLog.Slices

// ─── Canonical timestamps ─────────────────────────────────────────────────────

let T = DateTimeOffset(2025, 6, 1, 14, 0, 0, TimeSpan.Zero)

// ─── Shared fixtures ──────────────────────────────────────────────────────────

let locationEvent =
    { Name       = "LaundryLocationCaptured"
      OccurredAt = DateTimeOffset.MinValue
      Data       = { Location = "Love's #123 - Springfield, OH" } }

let expenseEvent (mt: MachineType) (qty: int option) (price: decimal option) (total: decimal) (pm: PaymentMethod) (at: DateTimeOffset) =
    { Name       = "LaundryExpenseLogged"
      OccurredAt = at
      Data       = { Location    = "Love's #123 - Springfield, OH"
                     MachineType = mt
                     Quantity    = qty
                     UnitPrice   = price
                     LineTotal   = total
                     Payment     = pm
                     Note        = None } }

// ─── PATH1 — Fresh Launch — No Location — First Entry ─────────────────────────

let setLocationSlice : CommandSlice<Actor, LaundryLocationCaptured, SetLocationCommand, LaundryLocationCaptured> =
    { Actor   = driver
      Command = { Name = "SetLocation"; IssuedBy = driver; Data = { Location = "Love's #123 - Springfield, OH" } }
      Handler = setLocationHandler
      GWT =
        { Given = []
          When  = { Name = "SetLocation"; IssuedBy = driver; Data = { Location = "Love's #123 - Springfield, OH" } }
          Then  = [ { Name       = "LaundryLocationCaptured"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location = "Love's #123 - Springfield, OH" } } ] } }

let logExpenseSlicePath1 : CommandSlice<Actor, LaundryLocationCaptured, LogExpenseCommand, LaundryExpenseLogged> =
    let cmd = { MachineType = Washer; Quantity = Some 1; UnitPrice = Some 3.00M; Amount = None; Payment = Cash; Note = None }
    { Actor   = driver
      Command = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
      Handler = logExpenseHandler
      GWT =
        { Given = [ locationEvent ]
          When  = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
          Then  = [ { Name       = "LaundryExpenseLogged"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location    = "Love's #123 - Springfield, OH"
                                     MachineType = Washer
                                     Quantity    = Some 1
                                     UnitPrice   = Some 3.00M
                                     LineTotal   = 3.00M
                                     Payment     = Cash
                                     Note        = None } } ] } }

let recentExpensesSlicePath1 : ViewSlice<LaundryExpenseLogged, RecentExpensesCriteria, RecentExpensesView, Actor> =
    let ev = expenseEvent Washer (Some 1) (Some 3.00M) 3.00M Cash T
    { Events  = []
      View    = { Name = "RecentExpenses"; Data = { SessionTotal = 0M; Entries = [] } }
      Handler = recentExpensesHandler
      GWT =
        { Given = [ ev ]
          When  = { QueryTime = T.AddMinutes(30.0) }
          Then  = { Name = "RecentExpenses"
                    Data = { SessionTotal = 3.00M
                             Entries = [ { LineTotal  = 3.00M
                                           Detail     = "1 Washer @ $3.00 • Cash"
                                           OccurredAt = T } ] } } }
      Actor   = driver }

// ─── PATH2 — Continue Session — Location Set — Dryer Entry ────────────────────
// Chains from PATH1. Given context: location already captured, washer already logged.

let logExpenseSlicePath2 : CommandSlice<Actor, LaundryLocationCaptured, LogExpenseCommand, LaundryExpenseLogged> =
    let cmd = { MachineType = Dryer; Quantity = Some 1; UnitPrice = Some 2.50M; Amount = None; Payment = Cash; Note = None }
    { Actor   = driver
      Command = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
      Handler = logExpenseHandler
      GWT =
        { Given = [ locationEvent ]
          When  = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
          Then  = [ { Name       = "LaundryExpenseLogged"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location    = "Love's #123 - Springfield, OH"
                                     MachineType = Dryer
                                     Quantity    = Some 1
                                     UnitPrice   = Some 2.50M
                                     LineTotal   = 2.50M
                                     Payment     = Cash
                                     Note        = None } } ] } }

// Washer ($3.00) at T, Dryer ($2.50) at T+30min — session total $5.50, newest first
let recentExpensesSlicePath2 : ViewSlice<LaundryExpenseLogged, RecentExpensesCriteria, RecentExpensesView, Actor> =
    let evWasher = expenseEvent Washer (Some 1) (Some 3.00M) 3.00M Cash T
    let evDryer  = expenseEvent Dryer  (Some 1) (Some 2.50M) 2.50M Cash (T.AddMinutes(30.0))
    { Events  = []
      View    = { Name = "RecentExpenses"; Data = { SessionTotal = 0M; Entries = [] } }
      Handler = recentExpensesHandler
      GWT =
        { Given = [ evWasher; evDryer ]
          When  = { QueryTime = T.AddMinutes(60.0) }
          Then  = { Name = "RecentExpenses"
                    Data = { SessionTotal = 5.50M
                             Entries = [ { LineTotal  = 2.50M
                                           Detail     = "1 Dryer @ $2.50 • Cash"
                                           OccurredAt = T.AddMinutes(30.0) }
                                         { LineTotal  = 3.00M
                                           Detail     = "1 Washer @ $3.00 • Cash"
                                           OccurredAt = T } ] } } }
      Actor   = driver }

// ─── Additional GWT cases — slice spec coverage beyond PATH1 and PATH2 ────────

let logExpenseSliceCard : CommandSlice<Actor, LaundryLocationCaptured, LogExpenseCommand, LaundryExpenseLogged> =
    let cmd = { MachineType = Washer; Quantity = Some 2; UnitPrice = Some 3.75M; Amount = None; Payment = Card "Business SPARK"; Note = None }
    { Actor   = driver
      Command = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
      Handler = logExpenseHandler
      GWT =
        { Given = [ locationEvent ]
          When  = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
          Then  = [ { Name       = "LaundryExpenseLogged"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location    = "Love's #123 - Springfield, OH"
                                     MachineType = Washer
                                     Quantity    = Some 2
                                     UnitPrice   = Some 3.75M
                                     LineTotal   = 7.50M
                                     Payment     = Card "Business SPARK"
                                     Note        = None } } ] } }

let logExpenseSliceSupplies : CommandSlice<Actor, LaundryLocationCaptured, LogExpenseCommand, LaundryExpenseLogged> =
    let cmd = { MachineType = Supplies; Quantity = None; UnitPrice = None; Amount = Some 2.50M; Payment = Cash; Note = Some "bleach packet" }
    { Actor   = driver
      Command = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
      Handler = logExpenseHandler
      GWT =
        { Given = [ locationEvent ]
          When  = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
          Then  = [ { Name       = "LaundryExpenseLogged"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location    = "Love's #123 - Springfield, OH"
                                     MachineType = Supplies
                                     Quantity    = None
                                     UnitPrice   = None
                                     LineTotal   = 2.50M
                                     Payment     = Cash
                                     Note        = Some "bleach packet" } } ] } }

// Two entries per spec — 2×Washer + 2×Dryer, card payment, 65 minutes apart
let recentExpensesSliceTwoEntries : ViewSlice<LaundryExpenseLogged, RecentExpensesCriteria, RecentExpensesView, Actor> =
    let ev1 = expenseEvent Washer (Some 2) (Some 3.75M) 7.50M (Card "Business SPARK") T
    let ev2 = expenseEvent Dryer  (Some 2) (Some 2.50M) 5.00M (Card "Business SPARK") (T.AddHours(1.0))
    { Events  = []
      View    = { Name = "RecentExpenses"; Data = { SessionTotal = 0M; Entries = [] } }
      Handler = recentExpensesHandler
      GWT =
        { Given = [ ev1; ev2 ]
          When  = { QueryTime = T.AddMinutes(65.0) }
          Then  = { Name = "RecentExpenses"
                    Data = { SessionTotal = 12.50M
                             Entries = [ { LineTotal  = 5.00M
                                           Detail     = "2 Dryers @ $2.50 • Card · Business SPARK"
                                           OccurredAt = T.AddHours(1.0) }
                                         { LineTotal  = 7.50M
                                           Detail     = "2 Washers @ $3.75 • Card · Business SPARK"
                                           OccurredAt = T } ] } } }
      Actor   = driver }

// Gap exceeds 3h — new session begins, prior entry not shown
let recentExpensesSliceGap : ViewSlice<LaundryExpenseLogged, RecentExpensesCriteria, RecentExpensesView, Actor> =
    let evOld = expenseEvent Washer (Some 1) (Some 3.00M) 3.00M Cash T
    let evNew = expenseEvent Dryer  (Some 1) (Some 2.00M) 2.00M Cash (T.AddHours(4.0))
    { Events  = []
      View    = { Name = "RecentExpenses"; Data = { SessionTotal = 0M; Entries = [] } }
      Handler = recentExpensesHandler
      GWT =
        { Given = [ evOld; evNew ]
          When  = { QueryTime = T.AddHours(4.0) }
          Then  = { Name = "RecentExpenses"
                    Data = { SessionTotal = 2.00M
                             Entries = [ { LineTotal  = 2.00M
                                           Detail     = "1 Dryer @ $2.00 • Cash"
                                           OccurredAt = T.AddHours(4.0) } ] } } }
      Actor   = driver }

// ─── Test suite — organized by PATH ───────────────────────────────────────────

let allTests =
    testList "LaundryLog" [

        testList "PATH1 — Fresh Launch — No Location — First Entry" [
            commandSliceToTest "SetLocation — fresh install, free-form location" setLocationSlice
            commandSliceToTest "LogExpense — single washer, cash"               logExpenseSlicePath1
            viewSliceToTest   "RecentExpenses — one entry in session"           recentExpensesSlicePath1
        ]

        testList "PATH2 — Continue Session — Location Set — Dryer Entry" [
            commandSliceToTest "LogExpense — single dryer, cash"                          logExpenseSlicePath2
            viewSliceToTest   "RecentExpenses — washer + dryer, session total $5.50"     recentExpensesSlicePath2
        ]

        testList "Slice spec coverage — additional GWT cases" [
            commandSliceToTest "LogExpense — multiple washers, named card"                logExpenseSliceCard
            commandSliceToTest "LogExpense — supplies with note, cash"                   logExpenseSliceSupplies
            viewSliceToTest   "RecentExpenses — two entries per spec (2×Washer+2×Dryer)" recentExpensesSliceTwoEntries
            viewSliceToTest   "RecentExpenses — gap exceeds 3h, new session begins"      recentExpensesSliceGap
        ]

    ]
