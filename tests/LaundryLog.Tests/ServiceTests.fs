module LaundryLog.Tests.ServiceTests

open System
open Expecto
open Stratum
open LaundryLog
open LaundryLog.Service

let private run a = Async.RunSynchronously a

// ─── PATH1 — SetLocation → LogExpense (Washer) → RecentExpenses ───────────────

let path1IntegrationTests =
    testList "PATH1 — Fresh Launch — No Location — First Entry" [

        testCase "SetLocation stores event; subsequent LogExpense reads location from store" <| fun () ->
            let store = InMemory.create()
            let sid   = localStream

            let setResult = setLocation store sid { Location = "Love's #123 - Springfield, OH" } |> run
            Expect.isOk setResult "SetLocation should succeed"

            let logResult = logExpense store sid { MachineType = Washer; Quantity = Some 1; UnitPrice = Some 3.00M; Amount = None; Payment = Cash; Note = None } |> run
            Expect.isOk logResult "LogExpense should succeed"

            let view = recentExpenses store sid (DateTimeOffset.UtcNow) |> run
            Expect.equal view.SessionTotal 3.00M "session total"
            Expect.equal (List.length view.Entries) 1 "one entry"
            Expect.equal view.Entries.[0].LineTotal 3.00M "line total"
            Expect.equal view.Entries.[0].Detail "1 Washer @ $3.00 • Cash" "detail line"

        testCase "SetLocation with invalid characters returns Error" <| fun () ->
            let store  = InMemory.create()
            let result = setLocation store localStream { Location = "Love's <script>" } |> run
            Expect.isError result "invalid chars should be rejected"

        testCase "SetLocation with empty location returns Error" <| fun () ->
            let store  = InMemory.create()
            let result = setLocation store localStream { Location = "" } |> run
            Expect.isError result "empty location should be rejected"

        testCase "RecentExpenses on empty store returns empty view" <| fun () ->
            let store = InMemory.create()
            let view  = recentExpenses store localStream DateTimeOffset.UtcNow |> run
            Expect.equal view.SessionTotal 0M "no total"
            Expect.isEmpty view.Entries "no entries"

    ]

// ─── PATH2 — Continue Session — Location Set — Dryer Entry ────────────────────

let path2IntegrationTests =
    testList "PATH2 — Continue Session — Location Set — Dryer Entry" [

        testCase "Washer then Dryer in session — correct totals, newest first" <| fun () ->
            let store = InMemory.create()
            let sid   = localStream

            setLocation store sid { Location = "Love's #123 - Springfield, OH" } |> run |> ignore
            logExpense  store sid { MachineType = Washer; Quantity = Some 1; UnitPrice = Some 3.00M; Amount = None; Payment = Cash; Note = None } |> run |> ignore
            logExpense  store sid { MachineType = Dryer;  Quantity = Some 1; UnitPrice = Some 2.50M; Amount = None; Payment = Cash; Note = None } |> run |> ignore

            let view = recentExpenses store sid DateTimeOffset.UtcNow |> run
            Expect.equal view.SessionTotal 5.50M "session total $5.50"
            Expect.equal (List.length view.Entries) 2 "two entries"
            // Newest first — dryer logged second so it appears first
            Expect.equal view.Entries.[0].Detail "1 Dryer @ $2.50 • Cash"  "first entry is dryer"
            Expect.equal view.Entries.[1].Detail "1 Washer @ $3.00 • Cash" "second entry is washer"

        testCase "streams are independent — two drivers do not share entries" <| fun () ->
            let store   = InMemory.create()
            let stream1 = StreamId "laundrylog:driver-1"
            let stream2 = StreamId "laundrylog:driver-2"

            setLocation store stream1 { Location = "Love's #123 - Springfield, OH" } |> run |> ignore
            logExpense  store stream1 { MachineType = Washer; Quantity = Some 1; UnitPrice = Some 3.00M; Amount = None; Payment = Cash; Note = None } |> run |> ignore

            setLocation store stream2 { Location = "Pilot #456 - Columbus, OH" } |> run |> ignore
            logExpense  store stream2 { MachineType = Dryer; Quantity = Some 2; UnitPrice = Some 2.00M; Amount = None; Payment = Cash; Note = None } |> run |> ignore

            let view1 = recentExpenses store stream1 DateTimeOffset.UtcNow |> run
            let view2 = recentExpenses store stream2 DateTimeOffset.UtcNow |> run

            Expect.equal view1.SessionTotal 3.00M "driver 1 total"
            Expect.equal view2.SessionTotal 4.00M "driver 2 total"
            Expect.equal view1.Entries.[0].Detail "1 Washer @ $3.00 • Cash" "driver 1 entry"
            Expect.equal view2.Entries.[0].Detail "2 Dryers @ $2.00 • Cash" "driver 2 entry"

    ]

// ─── All service tests ────────────────────────────────────────────────────────

let allServiceTests =
    testList "LaundryLog.Service" [
        path1IntegrationTests
        path2IntegrationTests
    ]
