module LaundryLog.Slices

open System
open EventModeling
open LaundryLog

// ─── Actors ───────────────────────────────────────────────────────────────────

let driver = { Name = "Driver"; Kind = Human "Driver"; Swimlane = None }

// ─── Format helpers ───────────────────────────────────────────────────────────

let private machineLabel (mt: MachineType) (qty: int) =
    match mt, qty with
    | Washer,   1 -> "Washer"
    | Washer,   _ -> "Washers"
    | Dryer,    1 -> "Dryer"
    | Dryer,    _ -> "Dryers"
    | Supplies, _ -> "Supplies"

let private formatPaymentDetail (pm: PaymentMethod) =
    match pm with
    | Cash          -> "Cash"
    | Card   name   -> $"Card · {name}"
    | App    name   -> $"App · {name}"
    | Points name   -> $"Points · {name}"

let formatDetail (data: LaundryExpenseLogged) =
    match data.MachineType with
    | Supplies ->
        $"Supplies ${data.LineTotal:F2} • {formatPaymentDetail data.Payment}"
    | _ ->
        let qty   = data.Quantity  |> Option.defaultValue 0
        let price = data.UnitPrice |> Option.defaultValue 0M
        let label = machineLabel data.MachineType qty
        $"{qty} {label} @ ${price:F2} • {formatPaymentDetail data.Payment}"

// ─── SetLocation ──────────────────────────────────────────────────────────────

let private isAllowedLocationChar (c: char) =
    Char.IsAsciiLetterOrDigit c
    || c = ' ' || c = '-' || c = '\'' || c = '.' || c = ',' || c = '&' || c = '#'

let setLocationHandler : CommandHandler<LaundryLocationCaptured, SetLocationCommand, LaundryLocationCaptured> =
    fun _given cmd ->
        let loc = cmd.Data.Location.Trim()
        if loc.Length = 0 then
            Error "Location must not be empty"
        elif loc |> Seq.exists (isAllowedLocationChar >> not) then
            Error "Location may contain only ASCII letters, digits, space, hyphen, apostrophe, dot, comma, ampersand, and hash"
        else
            Ok [ { Name = "LaundryLocationCaptured"; OccurredAt = DateTimeOffset.UtcNow; Data = { Location = loc } } ]

// ─── LogExpense ───────────────────────────────────────────────────────────────

let logExpenseHandler : CommandHandler<LaundryLocationCaptured, LogExpenseCommand, LaundryExpenseLogged> =
    fun given cmd ->
        let location =
            given
            |> List.tryLast
            |> Option.map (fun e -> e.Data.Location)
            |> Option.defaultValue ""
        let d = cmd.Data
        let lineTotal =
            match d.MachineType with
            | Supplies -> d.Amount |> Option.defaultValue 0M
            | _        ->
                let qty   = d.Quantity  |> Option.defaultValue 0
                let price = d.UnitPrice |> Option.defaultValue 0M
                decimal qty * price
        Ok [ { Name       = "LaundryExpenseLogged"
               OccurredAt = DateTimeOffset.UtcNow
               Data       = {
                   Location    = location
                   MachineType = d.MachineType
                   Quantity    = d.Quantity
                   UnitPrice   = d.UnitPrice
                   LineTotal   = lineTotal
                   Payment     = d.Payment
                   Note        = d.Note } } ]

// ─── RecentExpenses ───────────────────────────────────────────────────────────

let recentExpensesHandler : EventHandler<LaundryExpenseLogged, RecentExpensesCriteria, RecentExpensesView> =
    fun events criteria ->
        let threeHours = TimeSpan.FromHours(3.0)
        let sorted =
            events
            |> List.filter (fun e -> e.OccurredAt <= criteria.QueryTime)
            |> List.sortBy (fun e -> e.OccurredAt)
        let sessionEvents =
            sorted
            |> List.fold (fun acc e ->
                match acc with
                | [] -> [e]
                | _  ->
                    let lastTime = (List.last acc).OccurredAt
                    if e.OccurredAt - lastTime > threeHours then [e]
                    else acc @ [e]
            ) []
        let entries =
            sessionEvents
            |> List.rev
            |> List.map (fun e ->
                { LineTotal  = e.Data.LineTotal
                  Detail     = formatDetail e.Data
                  OccurredAt = e.OccurredAt })
        let sessionTotal = sessionEvents |> List.sumBy (fun e -> e.Data.LineTotal)
        { Name = "RecentExpenses"
          Data = { SessionTotal = sessionTotal; Entries = entries } }
