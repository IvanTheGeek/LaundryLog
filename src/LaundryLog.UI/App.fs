module LaundryLog.UI.App

open System.Globalization
open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components
open Stratum
open LaundryLog.UI
open LaundryLog.UI.Components
open LaundryLog.UI.Store

let private locationInput  = ComponentBuilder<LocationInput>()
let private stepper        = ComponentBuilder<Stepper>()
let private machineChips   = ComponentBuilder<MachineTypeChips>()
let private moneyInput     = ComponentBuilder<MoneyInput>()
let private paymentChips   = ComponentBuilder<PaymentChips>()
let private lineTotal      = ComponentBuilder<LineTotalDisplay>()

let private ic = CultureInfo.InvariantCulture

type AppComponent() =
    inherit FunComponent()

    // ── Form state ────────────────────────────────────────────────────
    let mutable locationText  = ""
    let mutable machineType   : MachineType option = None
    let mutable quantity      = 1
    let mutable unitPrice     : decimal option = None
    let mutable amount        : decimal option = None
    let mutable paymentKind   : PaymentKind option = None
    let mutable paymentName   = ""
    let mutable submitted     = false

    // ── Session state (from STRATUM) ──────────────────────────────────
    let mutable sessionEntries : SessionEntry list = []

    override this.Render() =

        // ── Handlers ─────────────────────────────────────────────────
        let handleMachineType mt =
            machineType <- Some mt
            quantity    <- 1
            unitPrice   <- None
            amount      <- None
            this.StateHasChanged()

        let handlePaymentKind kind =
            paymentKind <- Some kind
            paymentName <- ""
            this.StateHasChanged()

        let handleQuantity cmd =
            match cmd with
            | Increment     -> quantity <- min 9 (quantity + 1)
            | Decrement     -> quantity <- max 1 (quantity - 1)
            | DirectEntry n -> quantity <- n
            this.StateHasChanged()

        // ── canSubmit (per UX spec) ───────────────────────────────────
        let priceReady =
            match machineType with
            | Some Supplies -> amount    |> Option.exists (fun v -> v > 0m)
            | Some _        -> unitPrice |> Option.exists (fun v -> v > 0m)
            | None          -> false
        let paymentReady =
            match paymentKind with
            | None        -> false
            | Some Cash   -> true
            | Some _      -> paymentName.Length > 0
        let locationReady = locationText.Length > 0
        let typeReady     = machineType.IsSome
        let canSubmit     = locationReady && typeReady && priceReady && paymentReady

        // ── Submit ────────────────────────────────────────────────────
        let handleSubmit _ =
            if canSubmit then
                let entryTotal =
                    match machineType with
                    | Some Supplies -> amount    |> Option.defaultValue 0m
                    | Some _        -> unitPrice |> Option.defaultValue 0m |> (*) (decimal quantity)
                    | None          -> 0m

                let data : LaundryExpenseData = {
                    Location    = locationText
                    MachineType = match machineType with
                                  | Some Washer   -> "Washer"
                                  | Some Dryer    -> "Dryer"
                                  | Some Supplies -> "Supplies"
                                  | None          -> ""
                    Quantity    = match machineType with Some Supplies -> 0 | _ -> quantity
                    UnitPrice   = match machineType with Some Supplies -> 0m | _ -> unitPrice |> Option.defaultValue 0m
                    LineTotal   = entryTotal
                    Payment     = match paymentKind with
                                  | Some Cash   -> "Cash"
                                  | Some Card   -> "Card"
                                  | Some App    -> "App"
                                  | Some Points -> "Points"
                                  | None        -> ""
                    PaymentName = paymentName
                }

                task {
                    let! _ = store.Append streamId [buildEvent data] |> Async.StartAsTask
                    let! events = store.Read streamId Start |> Async.StartAsTask
                    // Update session view
                    sessionEntries <- projectSession events
                    // Reset form — payment is sticky (per spec)
                    machineType <- None
                    quantity    <- 1
                    unitPrice   <- None
                    amount      <- None
                    submitted   <- true
                    this.StateHasChanged()
                    do! System.Threading.Tasks.Task.Delay(2000)
                    submitted <- false
                    this.StateHasChanged()
                } |> ignore

        // ── Derived ───────────────────────────────────────────────────
        let entryTotal =
            match machineType with
            | Some Supplies -> amount
            | Some _        -> unitPrice |> Option.map (fun p -> decimal quantity * p)
            | None          -> None

        let sessionTotal = sessionEntries |> List.sumBy (fun e -> e.LineTotal)

        // ── Submit button status chips ────────────────────────────────
        let statusChip (chipLabel: string) (isReady: bool) =
            span {
                class' "ll-status-chip"
                chipLabel
                if isReady then
                    strong { class' "ll-status-chip__mark--ready"; " ✓" }
                else
                    strong { class' "ll-status-chip__mark--missing"; " ✗" }
            }

        // ── Render ────────────────────────────────────────────────────
        div {
            style' $"font-family: {Tokens.Font.Family.Body}; max-width: 360px; margin: 2rem auto; padding: 1.5rem;"

            locationInput {
                "Text"          => locationText
                "OnTextChanged" => (fun s -> locationText <- s; this.StateHasChanged())
            }
            div { style' "margin-top: 1.5rem;" }
            machineChips {
                "Selected"   => machineType
                "OnCommand"  => handleMachineType
            }
            match machineType with
            | None -> ()
            | Some Supplies ->
                div {
                    style' "margin-top: 1.5rem;"
                    moneyInput {
                        "Value"     => amount
                        "OnCommand" => (fun v -> amount <- v; this.StateHasChanged())
                    }
                }
            | Some _ ->
                div {
                    style' "margin-top: 1.5rem;"
                    stepper {
                        "Value"     => quantity
                        "Min"       => 1
                        "Max"       => (Some 9 : int option)
                        "OnCommand" => handleQuantity
                    }
                    div { style' "margin-top: 1rem;" }
                    moneyInput {
                        "Value"     => unitPrice
                        "OnCommand" => (fun v -> unitPrice <- v; this.StateHasChanged())
                    }
                }
            match machineType with
            | None -> ()
            | Some _ ->
                div {
                    style' "margin-top: 1.5rem;"
                    lineTotal { "Total" => entryTotal }
                }
            match machineType with
            | None -> ()
            | Some _ ->
                div {
                    style' "margin-top: 1.5rem;"
                    paymentChips {
                        "SelectedKind"   => paymentKind
                        "DetailName"     => paymentName
                        "OnKindCommand"  => handlePaymentKind
                        "OnNameCommand"  => (fun s -> paymentName <- s; this.StateHasChanged())
                    }
                }

            // ── Submit button ─────────────────────────────────────────
            div {
                style' "margin-top: 2rem;"
                if submitted then
                    button {
                        class' "ll-btn ll-btn--success"
                        disabled true
                        "✓ Logged!"
                    }
                elif canSubmit then
                    button {
                        class' "ll-btn ll-btn--primary"
                        onclick handleSubmit
                        "Log Expense"
                    }
                else
                    button {
                        class' "ll-btn ll-btn--disabled"
                        disabled true
                        div {
                            style' "display:flex;gap:8px;flex-wrap:wrap;justify-content:center;"
                            statusChip "📍 Location" locationReady
                            statusChip "🌊 Type"     typeReady
                            statusChip "💳 Payment"  paymentReady
                        }
                    }
            }

            // ── Session bar + entries (appear after first submit) ─────
            if sessionEntries.Length > 0 then
                div {
                    style' "margin-top: 2rem;"
                    p {
                        style' $"font-size:{Tokens.Font.Size.Xs};font-weight:{Tokens.Font.Weight.Semibold};letter-spacing:0.1em;text-transform:uppercase;color:{Tokens.Color.Text.Muted};margin:0 0 0.5rem 0;"
                        "Today's Entries"
                    }
                    div {
                        class' "ll-session-bar"
                        style' "margin-bottom: 0.5rem;"
                        span { class' "ll-session-bar__label"; "Session Total" }
                        span { class' "ll-session-bar__value"; "$" + sessionTotal.ToString("F2", ic) }
                    }
                    div {
                        style' "display:flex;flex-direction:column;gap:0.5rem;"
                        for entry in sessionEntries do
                            div {
                                class' "ll-entry-card ll-entry-card--logged"
                                div {
                                    class' "ll-entry-card__top"
                                    span { class' "ll-entry-card__amount"; "$" + entry.LineTotal.ToString("F2", ic) }
                                    span { class' "ll-entry-card__time"; entry.OccurredAt.ToLocalTime().ToString("h:mm tt", ic) }
                                }
                                p {
                                    class' "ll-entry-card__detail"
                                    style' "margin: 0.25rem 0 0;"
                                    entry.Detail
                                }
                            }
                    }
                }
        }
