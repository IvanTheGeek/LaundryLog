module LaundryLog.Service
#nowarn "3261" // Deserialize<'T> returns 'T|null in .NET 10 nullable mode; data we write cannot be null

open System
open System.Text.Json
open EventModeling
open LaundryLog
open LaundryLog.Slices
open Stratum

// ─── Storage DTOs ─────────────────────────────────────────────────────────────
// Flat records with only primitives and Nullable<T> — System.Text.Json native.
// F# discriminated unions and option types are mapped explicitly at the boundary.

[<CLIMutable>]
type LocationCapturedDto = {
    Location : string
}

[<CLIMutable>]
type ExpenseLoggedDto = {
    Location     : string
    MachineType  : string            // "Washer" | "Dryer" | "Supplies"
    Quantity     : Nullable<int>
    UnitPrice    : Nullable<decimal>
    LineTotal    : decimal
    PaymentKind  : string            // "Cash" | "Card" | "App" | "Points"
    PaymentName  : string            // empty string when not applicable
    Note         : string            // empty string when not applicable
}

// ─── Domain ↔ DTO mapping ─────────────────────────────────────────────────────

let private machineTypeToString = function
    | Washer   -> "Washer"
    | Dryer    -> "Dryer"
    | Supplies -> "Supplies"

let private machineTypeFromString = function
    | "Washer"   -> Washer
    | "Dryer"    -> Dryer
    | "Supplies" -> Supplies
    | s          -> failwith $"Unknown MachineType: {s}"

let private paymentToDto (pm: PaymentMethod) =
    match pm with
    | Cash          -> "Cash",   ""
    | Card   name   -> "Card",   name
    | App    name   -> "App",    name
    | Points name   -> "Points", name

let private paymentFromDto (kind: string) (name: string) =
    match kind with
    | "Cash"   -> Cash
    | "Card"   -> Card   name
    | "App"    -> App    name
    | "Points" -> Points name
    | s        -> failwith $"Unknown PaymentKind: {s}"

let private expenseToDto (e: LaundryExpenseLogged) : ExpenseLoggedDto =
    let kind, name = paymentToDto e.Payment
    { Location     = e.Location
      MachineType  = machineTypeToString e.MachineType
      Quantity     = e.Quantity  |> Option.map Nullable |> Option.defaultValue (Nullable())
      UnitPrice    = e.UnitPrice |> Option.map Nullable |> Option.defaultValue (Nullable())
      LineTotal    = e.LineTotal
      PaymentKind  = kind
      PaymentName  = name
      Note         = e.Note |> Option.defaultValue "" }

let private dtoToExpense (dto: ExpenseLoggedDto) : LaundryExpenseLogged =
    { Location    = dto.Location
      MachineType = machineTypeFromString dto.MachineType
      Quantity    = if dto.Quantity.HasValue    then Some dto.Quantity.Value    else None
      UnitPrice   = if dto.UnitPrice.HasValue   then Some dto.UnitPrice.Value   else None
      LineTotal   = dto.LineTotal
      Payment     = paymentFromDto dto.PaymentKind dto.PaymentName
      Note        = if String.IsNullOrEmpty dto.Note then None else Some dto.Note }

// ─── Serialization ────────────────────────────────────────────────────────────

let private toBytes<'T> (value: 'T) : byte[] =
    JsonSerializer.SerializeToUtf8Bytes<'T>(value)

let private fromBytes<'T> (bytes: byte[]) : 'T =
    JsonSerializer.Deserialize<'T>(ReadOnlySpan(bytes))

// ─── Stream identity ──────────────────────────────────────────────────────────
// All events for the local (single-device) driver live in one stream.
// Pass a different StreamId when multi-driver or multi-device is needed.

let localStream = StreamId "laundrylog:local"

// ─── Read helpers ─────────────────────────────────────────────────────────────

let private readLocations (store: IEventStore) (stream: StreamId) = async {
    let! stored = store.Read stream Start
    return
        stored
        |> List.filter  (fun e -> e.EventType = "LaundryLocationCaptured")
        |> List.map     (fun e ->
            let dto = fromBytes<LocationCapturedDto> e.Data
            { Name       = e.EventType
              OccurredAt = e.OccurredAt
              Data       = ({ Location = dto.Location } : LaundryLocationCaptured) })
}

let private readExpenses (store: IEventStore) (stream: StreamId) = async {
    let! stored = store.Read stream Start
    return
        stored
        |> List.filter  (fun e -> e.EventType = "LaundryExpenseLogged")
        |> List.map     (fun e ->
            let dto = fromBytes<ExpenseLoggedDto> e.Data
            { Name       = e.EventType
              OccurredAt = e.OccurredAt
              Data       = dtoToExpense dto })
}

// ─── Append helper ────────────────────────────────────────────────────────────

let private appendOne (store: IEventStore) (stream: StreamId) (corrId: Guid) (causationId: Guid) (eventType: string) (data: byte[]) = async {
    let evt = { EventId = Guid.CreateVersion7(); CorrelationId = corrId; CausationId = causationId; EventType = eventType; Data = data }
    let! _ = store.Append stream [evt]
    return ()
}

// ─── Commands ─────────────────────────────────────────────────────────────────

let setLocation (store: IEventStore) (stream: StreamId) (cmd: SetLocationCommand) = async {
    let! given   = readLocations store stream
    let  command = { Name = "SetLocation"; IssuedBy = driver; Data = cmd }
    match setLocationHandler given command with
    | Error msg -> return Error msg
    | Ok events ->
        let corrId = Guid.CreateVersion7()
        let cauId  = Guid.CreateVersion7()
        for e in events do
            let bytes = toBytes<LocationCapturedDto> { Location = e.Data.Location }
            do! appendOne store stream corrId cauId e.Name bytes
        return Ok ()
}

let logExpense (store: IEventStore) (stream: StreamId) (cmd: LogExpenseCommand) = async {
    let! given   = readLocations store stream
    let  command = { Name = "LogExpense"; IssuedBy = driver; Data = cmd }
    match logExpenseHandler given command with
    | Error msg -> return Error msg
    | Ok events ->
        let corrId = Guid.CreateVersion7()
        let cauId  = Guid.CreateVersion7()
        for e in events do
            let bytes = toBytes<ExpenseLoggedDto> (expenseToDto e.Data)
            do! appendOne store stream corrId cauId e.Name bytes
        return Ok ()
}

// ─── Queries ──────────────────────────────────────────────────────────────────

let recentExpenses (store: IEventStore) (stream: StreamId) (queryTime: DateTimeOffset) = async {
    let! expenses = readExpenses store stream
    let  view     = recentExpensesHandler expenses { QueryTime = queryTime }
    return view.Data
}
