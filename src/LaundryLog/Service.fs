module LaundryLog.Service

open System
open System.Globalization
open System.Text
open Nexus.Modeling
open LaundryLog
open LaundryLog.Slices
open Stratum

// ─── Domain ↔ string mappings ────────────────────────────────────────────────

let private machineTypeToString = function
    | Washer   -> "Washer"
    | Dryer    -> "Dryer"
    | Supplies -> "Supplies"

let private machineTypeFromString = function
    | "Washer"   -> Washer
    | "Dryer"    -> Dryer
    | "Supplies" -> Supplies
    | s          -> failwith $"Unknown MachineType: {s}"

let private paymentToStrings (pm: PaymentMethod) =
    match pm with
    | Cash        -> "Cash",   ""
    | Card   name -> "Card",   name
    | App    name -> "App",    name
    | Points name -> "Points", name

let private paymentFromStrings (kind: string) (name: string) =
    match kind with
    | "Cash"   -> Cash
    | "Card"   -> Card   name
    | "App"    -> App    name
    | "Points" -> Points name
    | s        -> failwith $"Unknown Payment: {s}"

// ─── TOML helpers ─────────────────────────────────────────────────────────────

// Escape backslash and double-quote for TOML basic strings.
let private tomlStr (s: string) =
    let escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"")
    $"\"{escaped}\""

let private decToStr (d: decimal) = d.ToString(CultureInfo.InvariantCulture)
let private parseDec (s: string)  = Decimal.Parse(s, CultureInfo.InvariantCulture)

// Parse a flat TOML document into a string map.
// Handles basic strings (quoted) and unquoted values (integers).
// Skips blank lines, comments, and section headers.
let private parseToml (text: string) : Map<string, string> =
    text.Split([| '\n'; '\r' |], StringSplitOptions.RemoveEmptyEntries)
    |> Array.choose (fun line ->
        let line = line.Trim()
        if line = "" || line.StartsWith('#') || line.StartsWith('[') then None
        else
            let eq = line.IndexOf(" = ")
            if eq = -1 then None
            else
                let key = line.[..eq - 1].Trim()
                let raw = line.[eq + 3..].Trim()
                let value =
                    if raw.StartsWith('"') && raw.EndsWith('"') && raw.Length >= 2 then
                        raw.[1..raw.Length - 2]
                            .Replace("\\\"", "\"")
                            .Replace("\\\\", "\\")
                    else raw
                Some (key, value))
    |> Map.ofArray

// ─── Serialization ────────────────────────────────────────────────────────────

let private locationToBytes (e: LaundryLocationCaptured) : byte[] =
    Encoding.UTF8.GetBytes($"Location = {tomlStr e.Location}\n")

let private locationFromBytes (bytes: byte[]) : LaundryLocationCaptured =
    let m = parseToml (Encoding.UTF8.GetString bytes)
    { Location = m["Location"] }

let private expenseToBytes (e: LaundryExpenseLogged) : byte[] =
    let lines = ResizeArray<string>()
    lines.Add $"Location    = {tomlStr e.Location}"
    lines.Add $"MachineType = {tomlStr (machineTypeToString e.MachineType)}"
    e.Quantity  |> Option.iter (fun q -> lines.Add $"Quantity    = {q}")
    e.UnitPrice |> Option.iter (fun p -> lines.Add $"UnitPrice   = {tomlStr (decToStr p)}")
    lines.Add $"LineTotal   = {tomlStr (decToStr e.LineTotal)}"
    let kind, name = paymentToStrings e.Payment
    lines.Add $"Payment     = {tomlStr kind}"
    if name <> "" then lines.Add $"PaymentName = {tomlStr name}"
    e.Note |> Option.iter (fun n -> lines.Add $"Note        = {tomlStr n}")
    Encoding.UTF8.GetBytes(String.concat "\n" lines + "\n")

let private expenseFromBytes (bytes: byte[]) : LaundryExpenseLogged =
    let m = parseToml (Encoding.UTF8.GetString bytes)
    { Location    = m["Location"]
      MachineType = machineTypeFromString m["MachineType"]
      Quantity    = Map.tryFind "Quantity"    m |> Option.map int
      UnitPrice   = Map.tryFind "UnitPrice"   m |> Option.map parseDec
      LineTotal   = parseDec m["LineTotal"]
      Payment     = paymentFromStrings m["Payment"] (Map.tryFind "PaymentName" m |> Option.defaultValue "")
      Note        = Map.tryFind "Note" m }

// ─── Stream identity ──────────────────────────────────────────────────────────

let localStream = StreamId "laundrylog:local"

// ─── Read helpers ─────────────────────────────────────────────────────────────

let private readLocations (store: IEventStore) (stream: StreamId) = async {
    let! stored = store.Read stream Start
    return
        stored
        |> List.filter (fun e -> e.EventType = "LaundryLocationCaptured")
        |> List.map    (fun e ->
            { Name       = e.EventType
              OccurredAt = e.OccurredAt
              Data       = locationFromBytes e.Data })
}

let private readExpenses (store: IEventStore) (stream: StreamId) = async {
    let! stored = store.Read stream Start
    return
        stored
        |> List.filter (fun e -> e.EventType = "LaundryExpenseLogged")
        |> List.map    (fun e ->
            { Name       = e.EventType
              OccurredAt = e.OccurredAt
              Data       = expenseFromBytes e.Data })
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
            do! appendOne store stream corrId cauId e.Name (locationToBytes e.Data)
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
            do! appendOne store stream corrId cauId e.Name (expenseToBytes e.Data)
        return Ok ()
}

// ─── Queries ──────────────────────────────────────────────────────────────────

let recentExpenses (store: IEventStore) (stream: StreamId) (queryTime: DateTimeOffset) = async {
    let! expenses = readExpenses store stream
    let  view     = recentExpensesHandler expenses { QueryTime = queryTime }
    return view.Data
}
