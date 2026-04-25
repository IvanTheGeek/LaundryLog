module LaundryLog.UI.App

open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components
open LaundryLog.UI
open LaundryLog.UI.Components

let private locationInput  = ComponentBuilder<LocationInput>()
let private stepper        = ComponentBuilder<Stepper>()
let private machineChips   = ComponentBuilder<MachineTypeChips>()
let private moneyInput     = ComponentBuilder<MoneyInput>()
let private paymentChips   = ComponentBuilder<PaymentChips>()
let private lineTotal      = ComponentBuilder<LineTotalDisplay>()

type AppComponent() =
    inherit FunComponent()

    let mutable locationText = ""
    let mutable machineType  : MachineType option  = None
    let mutable quantity     = 1
    let mutable unitPrice    : decimal option = None
    let mutable amount       : decimal option = None
    let mutable paymentKind  : PaymentKind option  = None
    let mutable paymentName  = ""
    let mutable submitted    = false

    override this.Render() =
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

        let handleSubmit _ =
            submitted <- true
            this.StateHasChanged()
            task {
                do! System.Threading.Tasks.Task.Delay(2000)
                submitted   <- false
                machineType <- None
                quantity    <- 1
                unitPrice   <- None
                amount      <- None
                paymentKind <- None
                paymentName <- ""
                this.StateHasChanged()
            } |> ignore

        let entryTotal =
            match machineType with
            | Some Supplies -> amount
            | Some _        -> unitPrice |> Option.map (fun p -> decimal quantity * p)
            | None          -> None

        let locationReady = locationText <> ""
        let typeReady     = machineType.IsSome
        let paymentReady  = paymentKind.IsSome

        let statusChip (chipLabel: string) (isReady: bool) =
            span {
                class' "ll-status-chip"
                chipLabel
                if isReady then
                    strong { class' "ll-status-chip__mark--ready"; " ✓" }
                else
                    strong { class' "ll-status-chip__mark--missing"; " ✗" }
            }

        div {
            style' "font-family: var(--cb-font-body, system-ui); max-width: 360px; margin: 2rem auto; padding: 1.5rem;"
            h1 {
                style' "color: var(--cb-text-accent, #7a4f1e); font-size: var(--cb-text-2xl, 1.9rem); margin-bottom: 0.25rem;"
                "LaundryLog"
            }
            p {
                style' "color: var(--cb-text-secondary, #6b5c4a); margin-bottom: 1.5rem;"
                "PATH1 · entry form · walking skeleton"
            }
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
            div {
                style' "margin-top: 2rem;"
                if submitted then
                    button {
                        class' "ll-btn ll-btn--success"
                        disabled true
                        "✓ Logged!"
                    }
                elif locationReady && typeReady && paymentReady then
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
                            statusChip "🌊 Type" typeReady
                            statusChip "💳 Payment" paymentReady
                        }
                    }
            }
        }
