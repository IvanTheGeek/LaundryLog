namespace LaundryLog.UI.Dev

open Fun.Blazor
open Fun.Blazor.Operators
open FnTools.FnHCI.UI.Blazor.Components
open LaundryLog.UI
open LaundryLog.UI.Components

type GalleryComponent() =
    inherit FunComponent()

    let chips = ComponentBuilder<MachineTypeChips>()
    let money = ComponentBuilder<MoneyInput>()
    let stepperCb = ComponentBuilder<Stepper>()

    // ── MachineTypeChips story state ──────────────────────────────────
    let mutable chipsNone     : MachineType option = None
    let mutable chipsWasher   : MachineType option = Some Washer
    let mutable chipsDryer    : MachineType option = Some Dryer
    let mutable chipsSupplies : MachineType option = Some Supplies

    // ── MoneyInput story state ────────────────────────────────────────
    let mutable moneyEmpty  : decimal option = None
    let mutable moneyFilled : decimal option = Some 3.75m

    // ── Stepper story state ───────────────────────────────────────────
    let mutable stepMin = 1
    let mutable stepMid = 5
    let mutable stepMax = 9

    override this.Render() =

        let card (label: string) (body: NodeRenderFragment) =
            div {
                style' "display:flex;flex-direction:column;gap:0.5rem;"
                div {
                    style' "border:1px solid var(--cb-border-default);border-radius:var(--cb-radius-md);padding:1.25rem 1rem;background:var(--cb-surface-raised);min-width:300px;"
                    body
                }
                span {
                    style' "font-size:0.6875rem;color:var(--cb-text-muted);text-transform:uppercase;letter-spacing:0.08em;font-weight:600;"
                    label
                }
            }

        let gallerySection (sectionId: string) (title: string) (body: NodeRenderFragment) =
            div {
                id sectionId
                style' "margin-bottom:3rem;scroll-margin-top:4.5rem;"
                h2 {
                    style' "font-size:var(--cb-text-lg,1.125rem);font-weight:var(--cb-weight-bold,700);color:var(--cb-text-primary);border-bottom:2px solid var(--cb-border-default);padding-bottom:0.5rem;margin-bottom:1.5rem;margin-top:0;"
                    title
                }
                div {
                    style' "display:flex;flex-wrap:wrap;gap:1.5rem;align-items:flex-start;"
                    body
                }
            }

        let stepCmd (get: unit -> int) (set: int -> unit) cmd =
            set (match cmd with
                 | Increment     -> min 9 (get() + 1)
                 | Decrement     -> max 1 (get() - 1)
                 | DirectEntry n -> n)
            this.StateHasChanged()

        div {
            style' "font-family:var(--cb-font-body,system-ui);min-height:100dvh;background:var(--cb-surface-base);"

            // ── Sticky header ────────────────────────────────────────────────
            div {
                style' "padding:1rem 1.5rem;border-bottom:1px solid var(--cb-border-default);background:var(--cb-surface-raised);display:flex;align-items:center;gap:1rem;position:sticky;top:0;z-index:10;"
                a {
                    href "/"
                    style' "font-size:var(--cb-text-sm,0.875rem);color:var(--cb-text-secondary);text-decoration:none;"
                    "← App"
                }
                span { style' "color:var(--cb-border-default);"; "|" }
                span {
                    style' "font-weight:var(--cb-weight-bold,700);color:var(--cb-text-primary);"
                    "Component Gallery"
                }
                span {
                    style' "margin-left:auto;font-size:var(--cb-text-xs,0.75rem);color:var(--cb-text-muted);"
                    "LaundryLog · dev"
                }
            }

            // ── Sidebar + main ───────────────────────────────────────────────
            div {
                style' "display:flex;"

                // Sidebar
                div {
                    style' "width:180px;flex-shrink:0;padding:1.5rem 1rem;border-right:1px solid var(--cb-border-default);position:sticky;top:57px;align-self:flex-start;height:calc(100dvh - 57px);overflow-y:auto;"
                    p {
                        style' "font-size:0.6875rem;color:var(--cb-text-muted);text-transform:uppercase;letter-spacing:0.08em;font-weight:600;margin:0 0 0.75rem 0;"
                        "Components"
                    }
                    div {
                        style' "display:flex;flex-direction:column;gap:0.25rem;"
                        a { href "#machine-chips"; style' "font-size:var(--cb-text-sm,0.875rem);color:var(--cb-text-secondary);text-decoration:none;padding:0.375rem 0.5rem;border-radius:var(--cb-radius-sm,0.25rem);display:block;"; "MachineTypeChips" }
                        a { href "#money-input";   style' "font-size:var(--cb-text-sm,0.875rem);color:var(--cb-text-secondary);text-decoration:none;padding:0.375rem 0.5rem;border-radius:var(--cb-radius-sm,0.25rem);display:block;"; "MoneyInput" }
                        a { href "#stepper";       style' "font-size:var(--cb-text-sm,0.875rem);color:var(--cb-text-secondary);text-decoration:none;padding:0.375rem 0.5rem;border-radius:var(--cb-radius-sm,0.25rem);display:block;"; "Stepper" }
                    }
                }

                // Main content
                div {
                    style' "flex:1;min-width:0;padding:2rem 1.5rem;"

                    gallerySection "machine-chips" "MachineTypeChips" (html.fragment [
                        card "None selected"
                            (chips { "Selected" => chipsNone; "OnCommand" => (fun mt -> chipsNone <- Some mt; this.StateHasChanged()) })
                        card "Washer selected"
                            (chips { "Selected" => chipsWasher; "OnCommand" => (fun mt -> chipsWasher <- Some mt; this.StateHasChanged()) })
                        card "Dryer selected"
                            (chips { "Selected" => chipsDryer; "OnCommand" => (fun mt -> chipsDryer <- Some mt; this.StateHasChanged()) })
                        card "Supplies selected"
                            (chips { "Selected" => chipsSupplies; "OnCommand" => (fun mt -> chipsSupplies <- Some mt; this.StateHasChanged()) })
                    ])

                    gallerySection "money-input" "MoneyInput" (html.fragment [
                        card "Empty"
                            (money { "Value" => moneyEmpty; "OnCommand" => (fun v -> moneyEmpty <- v; this.StateHasChanged()) })
                        card "Pre-filled $3.75"
                            (money { "Value" => moneyFilled; "OnCommand" => (fun v -> moneyFilled <- v; this.StateHasChanged()) })
                    ])

                    gallerySection "stepper" "Stepper" (html.fragment [
                        card "Starts at min (1)"
                            (stepperCb { "Value" => stepMin; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMin) (fun v -> stepMin <- v) })
                        card "Starts at mid (5)"
                            (stepperCb { "Value" => stepMid; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMid) (fun v -> stepMid <- v) })
                        card "Starts at max (9)"
                            (stepperCb { "Value" => stepMax; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMax) (fun v -> stepMax <- v) })
                    ])
                }
            }
        }
