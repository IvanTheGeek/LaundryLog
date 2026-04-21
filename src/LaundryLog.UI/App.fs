module LaundryLog.UI.App

open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components

let private stepper = ComponentBuilder<Stepper>()

type AppComponent() =
    inherit FunComponent()

    // Walking skeleton stand-in for the event store + projector.
    // In the full model: OnCommand dispatches to a command handler → event →
    // projector updates the View → View flows back as parameter.
    let mutable quantity = 1

    override this.Render() =
        let handleCommand cmd =
            match cmd with
            | Increment    -> quantity <- min 9 (quantity + 1)
            | Decrement    -> quantity <- max 1 (quantity - 1)
            | DirectEntry n -> quantity <- n
            this.StateHasChanged()

        div {
            style' "font-family: var(--cb-font-body, system-ui); max-width: 360px; margin: 2rem auto; padding: 1.5rem; background: var(--cb-surface-base, #f9f7f4);"
            h1 {
                style' "color: var(--cb-text-accent, #7a4f1e); font-size: var(--cb-text-2xl, 1.9rem); margin-bottom: 0.5rem;"
                "LaundryLog"
            }
            p {
                style' "color: var(--cb-text-secondary, #6b5c4a); margin-bottom: 2rem;"
                "FnHCI.UI.Blazor · Stepper component"
            }
            stepper {
                "Value"     => quantity
                "Min"       => 1
                "Max"       => (Some 9 : int option)
                "OnCommand" => handleCommand
            }
        }
