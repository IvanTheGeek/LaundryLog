module LaundryLog.UI.Store

open System
open System.Globalization
open System.Text.Json
open Stratum
open LaundryLog.UI

// ── Shared store + stream ──────────────────────────────────────────────────
let store    : IEventStore = InMemory.create()
let streamId : StreamId    = StreamId "ll-expenses"

let eventType = "LaundryExpenseLogged"

// ── Event construction ─────────────────────────────────────────────────────
let buildEvent (data: LaundryExpenseData) : EventToAppend =
    { EventId       = Guid.CreateVersion7()
      CorrelationId = Guid.NewGuid()
      CausationId   = Guid.NewGuid()
      EventType     = eventType
      Data          = JsonSerializer.SerializeToUtf8Bytes(data) }

// ── Session projection ─────────────────────────────────────────────────────
type SessionEntry = {
    LineTotal  : decimal
    Detail     : string
    OccurredAt : DateTimeOffset
}

let private threeHours = TimeSpan.FromHours(3.0)
let private ic         = CultureInfo.InvariantCulture

let private formatAmount (v: decimal) = "$" + v.ToString("F2", ic)

let private formatDetail (d: LaundryExpenseData) =
    let payDisplay =
        match d.Payment with
        | "Card"   -> "Card · "   + d.PaymentName
        | "App"    -> "App · "    + d.PaymentName
        | "Points" -> "Points · " + d.PaymentName
        | _        -> "Cash"
    match d.MachineType with
    | "Supplies" ->
        "Supplies " + formatAmount d.LineTotal + " • " + payDisplay
    | mt ->
        let plural = if d.Quantity = 1 then mt else mt + "s"
        string d.Quantity + " " + plural + " @ " + formatAmount d.UnitPrice + " • " + payDisplay

let projectSession (events: StoredEvent list) : SessionEntry list =
    // Explicit type on expenses avoids FS3566 field-name ambiguity with SessionEntry.OccurredAt
    let expenses : StoredEvent list =
        events
        |> List.filter (fun (e: StoredEvent) -> e.EventType = eventType)
        |> List.sortByDescending (fun (e: StoredEvent) -> e.OccurredAt)

    match expenses with
    | [] -> []
    | first :: rest ->
        let rec collect (prev: StoredEvent) (acc: StoredEvent list) (remaining: StoredEvent list) : StoredEvent list =
            match remaining with
            | [] -> acc
            | next :: tail ->
                if prev.OccurredAt - next.OccurredAt <= threeHours then
                    collect next (next :: acc) tail
                else
                    acc
        let sessionStored : StoredEvent list = collect first [first] rest
        sessionStored
        |> List.map (fun (e: StoredEvent) ->
            // Decode byte[] → string first to resolve Deserialize overload unambiguously
            let json = System.Text.Encoding.UTF8.GetString(e.Data)
            let data = JsonSerializer.Deserialize<LaundryExpenseData>(json)
            { LineTotal  = data.LineTotal
              Detail     = formatDetail data
              OccurredAt = e.OccurredAt })
