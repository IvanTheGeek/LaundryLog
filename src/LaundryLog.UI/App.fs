module LaundryLog.UI.App

open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components

// Module-level builder instance — avoids allocating on every Render() call
let private stepper = ComponentBuilder<Stepper>()

type AppComponent() =
    inherit FunComponent()

    override _.Render() =
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
                "InitialValue" => 1
                "Min" => 1
            }
        }
