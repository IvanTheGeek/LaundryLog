module LaundryLog.Tests.Tests

open System
open Expecto
open EventModeling
open EventModeling.Testing.Assert
open LaundryLog
open LaundryLog.Slices

// ─── Canonical timestamps ─────────────────────────────────────────────────────

let T = DateTimeOffset(2025, 6, 1, 14, 0, 0, TimeSpan.Zero)

// ─── SetLocation ──────────────────────────────────────────────────────────────

let setLocationSlice : CommandSlice<Actor, LaundryLocationCaptured, SetLocationCommand, LaundryLocationCaptured> =
    { Actor   = driver
      Command = { Name = "SetLocation"; IssuedBy = driver; Data = { Location = "Love's #123 - Springfield, OH" } }
      Handler = setLocationHandler
      GWT =
        { Given = []
          When  = { Name = "SetLocation"; IssuedBy = driver; Data = { Location = "Love's #123 - Springfield, OH" } }
          Then  = [ { Name = "LaundryLocationCaptured"
                      OccurredAt = DateTimeOffset.MinValue
                      Data       = { Location = "Love's #123 - Springfield, OH" } } ] } }

// ─── LogExpense ───────────────────────────────────────────────────────────────

let locationEvent =
    { Name       = "LaundryLocationCaptured"
      OccurredAt = DateTimeOffset.MinValue
      Data       = { Location = "Love's #123 - Springfield, OH" } }

// PATH1 — single washer, cash
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

// Multiple washers, named card
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

// Supplies — amount only, no quantity
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

// ─── RecentExpenses ───────────────────────────────────────────────────────────

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

// PATH1 — one entry in session
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

// Two entries in same session — newest first
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

// ─── All tests ────────────────────────────────────────────────────────────────

let allTests =
    testList "LaundryLog" [
        testList "SetLocation" [
            commandSliceToTest "PATH1 — fresh install, free-form location" setLocationSlice
        ]
        testList "LogExpense" [
            commandSliceToTest "PATH1 — single washer, cash"           logExpenseSlicePath1
            commandSliceToTest "multiple washers, named card"          logExpenseSliceCard
            commandSliceToTest "supplies with note, cash"              logExpenseSliceSupplies
        ]
        testList "RecentExpenses" [
            viewSliceToTest "PATH1 — one entry in session"             recentExpensesSlicePath1
            viewSliceToTest "two entries in same session, newest first" recentExpensesSliceTwoEntries
            viewSliceToTest "gap exceeds 3h — new session begins"      recentExpensesSliceGap
        ]
    ]
