module LaundryLog.UI.App

open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components
open LaundryLog.UI
open LaundryLog.UI.Components

let private stepper        = ComponentBuilder<Stepper>()
let private machineChips   = ComponentBuilder<MachineTypeChips>()
let private moneyInput     = ComponentBuilder<MoneyInput>()
let private paymentChips   = ComponentBuilder<PaymentChips>()
let private lineTotal      = ComponentBuilder<LineTotalDisplay>()

type AppComponent() =
    inherit FunComponent()

    let mutable machineType  : MachineType option  = None
    let mutable quantity     = 1
    let mutable unitPrice    : decimal option = None
    let mutable amount       : decimal option = None
    let mutable paymentKind  : PaymentKind option  = None
    let mutable paymentName  = ""

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

        let entryTotal =
            match machineType with
            | Some Supplies -> amount
            | Some _        -> unitPrice |> Option.map (fun p -> decimal quantity * p)
            | None          -> None

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
        }
