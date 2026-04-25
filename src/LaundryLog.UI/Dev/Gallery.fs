namespace LaundryLog.UI.Dev

open Fun.Blazor
open Fun.Blazor.Operators
open Microsoft.AspNetCore.Components
open FnTools.FnHCI.UI.Blazor.Components
open LaundryLog.UI
open LaundryLog.UI.Components

type GalleryComponent() =
    inherit FunComponent()

    // ── ComponentBuilders ─────────────────────────────────────────────
    let location  = ComponentBuilder<LocationInput>()
    let chips     = ComponentBuilder<MachineTypeChips>()
    let money     = ComponentBuilder<MoneyInput>()
    let stepperCb = ComponentBuilder<Stepper>()
    let payment   = ComponentBuilder<PaymentChips>()
    let total     = ComponentBuilder<LineTotalDisplay>()

    // ── LocationInput story state ─────────────────────────────────────
    let mutable locationEmpty  = ""
    let mutable locationFilled = "Love's #123 — Springfield, OH"

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

    // ── PaymentChips story state ──────────────────────────────────────
    let mutable payNone      : PaymentKind option = None
    let mutable payCash      : PaymentKind option = Some Cash
    let mutable payCard      : PaymentKind option = Some Card
    let mutable payCardName  = "Business SPARK"
    let mutable payApp       : PaymentKind option = Some App
    let mutable payAppName   = ""
    let mutable payPoints    : PaymentKind option = Some Points
    let mutable payPointsName = ""

    // ── Parameter — must come after all let bindings ──────────────────
    [<Parameter>] member val ActiveStory: string = "" with get, set

    override this.Render() =

        // Show this section when viewing all or when it is the active story
        let show sectionId =
            this.ActiveStory = "" || this.ActiveStory = sectionId

        let card (linkLabel: string) (onReset: unit -> unit) (body: NodeRenderFragment) =
            div {
                style' "display:flex;flex-direction:column;gap:0.5rem;"
                div {
                    style' "border:1px solid var(--cb-border-default);border-radius:var(--cb-radius-md);padding:1.25rem 1rem;background:var(--cb-surface-raised);min-width:300px;"
                    body
                }
                div {
                    style' "display:flex;align-items:center;justify-content:space-between;"
                    span {
                        style' "font-size:0.6875rem;color:var(--cb-text-muted);text-transform:uppercase;letter-spacing:0.08em;font-weight:600;"
                        linkLabel
                    }
                    button {
                        style' "font-size:0.6875rem;color:var(--cb-text-muted);background:none;border:none;cursor:pointer;padding:0.125rem 0.25rem;border-radius:2px;opacity:0.6;"
                        onclick (fun _ -> onReset ())
                        "↺ reset"
                    }
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

        // Tuple arg avoids CE parser ambiguity with two-string curried calls
        let navLink (sectionId: string, linkLabel: string) =
            let isActive = this.ActiveStory = sectionId
            let navStyle =
                "font-size:var(--cb-text-sm,0.875rem);text-decoration:none;padding:0.375rem 0.5rem;border-radius:var(--cb-radius-sm,0.25rem);display:block;" +
                if isActive then "color:var(--cb-text-accent);background:var(--cb-accent-subtle);font-weight:var(--cb-weight-semibold);"
                else "color:var(--cb-text-secondary);"
            a { href ("/dev/" + sectionId); style' navStyle; linkLabel }

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
                a {
                    href "/dev"
                    style' "font-weight:var(--cb-weight-bold,700);color:var(--cb-text-primary);text-decoration:none;"
                    "Component Gallery"
                }
                if this.ActiveStory <> "" then
                    span { style' "color:var(--cb-border-default);"; "/" }
                    span {
                        style' "color:var(--cb-text-accent);font-weight:var(--cb-weight-semibold);"
                        this.ActiveStory
                    }
                span {
                    style' "margin-left:auto;font-size:var(--cb-text-xs,0.75rem);color:var(--cb-text-muted);"
                    "LaundryLog · dev"
                }
            }

            // ── Sidebar + main ───────────────────────────────────────────────
            div {
                style' "display:flex;"

                // Sidebar — always visible; links navigate to isolated story views
                div {
                    style' "width:180px;flex-shrink:0;padding:1.5rem 1rem;border-right:1px solid var(--cb-border-default);position:sticky;top:57px;align-self:flex-start;height:calc(100dvh - 57px);overflow-y:auto;"
                    p {
                        style' "font-size:0.6875rem;color:var(--cb-text-muted);text-transform:uppercase;letter-spacing:0.08em;font-weight:600;margin:0 0 0.75rem 0;"
                        "Components"
                    }
                    div {
                        style' "display:flex;flex-direction:column;gap:0.25rem;"
                        navLink ("location-input", "LocationInput")
                        navLink ("machine-chips",  "MachineTypeChips")
                        navLink ("money-input",    "MoneyInput")
                        navLink ("stepper",        "Stepper")
                        navLink ("payment-chips",  "PaymentChips")
                        navLink ("line-total",     "LineTotalDisplay")
                    }
                }

                // Main content — shows all sections or just the active one
                div {
                    style' "flex:1;min-width:0;padding:2rem 1.5rem;"

                    if show "location-input" then
                        gallerySection "location-input" "LocationInput" (html.fragment [
                            card "Empty"
                                (fun () -> locationEmpty <- ""; this.StateHasChanged())
                                (location { "Text" => locationEmpty; "OnTextChanged" => (fun s -> locationEmpty <- s; this.StateHasChanged()) })
                            card "Pre-filled"
                                (fun () -> locationFilled <- "Love's #123 — Springfield, OH"; this.StateHasChanged())
                                (location { "Text" => locationFilled; "OnTextChanged" => (fun s -> locationFilled <- s; this.StateHasChanged()) })
                        ])

                    if show "machine-chips" then
                        gallerySection "machine-chips" "MachineTypeChips" (html.fragment [
                            card "None selected"
                                (fun () -> chipsNone <- None; this.StateHasChanged())
                                (chips { "Selected" => chipsNone; "OnCommand" => (fun mt -> chipsNone <- Some mt; this.StateHasChanged()) })
                            card "Washer selected"
                                (fun () -> chipsWasher <- Some Washer; this.StateHasChanged())
                                (chips { "Selected" => chipsWasher; "OnCommand" => (fun mt -> chipsWasher <- Some mt; this.StateHasChanged()) })
                            card "Dryer selected"
                                (fun () -> chipsDryer <- Some Dryer; this.StateHasChanged())
                                (chips { "Selected" => chipsDryer; "OnCommand" => (fun mt -> chipsDryer <- Some mt; this.StateHasChanged()) })
                            card "Supplies selected"
                                (fun () -> chipsSupplies <- Some Supplies; this.StateHasChanged())
                                (chips { "Selected" => chipsSupplies; "OnCommand" => (fun mt -> chipsSupplies <- Some mt; this.StateHasChanged()) })
                        ])

                    if show "money-input" then
                        gallerySection "money-input" "MoneyInput" (html.fragment [
                            card "Empty"
                                (fun () -> moneyEmpty <- None; this.StateHasChanged())
                                (money { "Value" => moneyEmpty; "OnCommand" => (fun v -> moneyEmpty <- v; this.StateHasChanged()) })
                            card "Pre-filled $3.75"
                                (fun () -> moneyFilled <- Some 3.75m; this.StateHasChanged())
                                (money { "Value" => moneyFilled; "OnCommand" => (fun v -> moneyFilled <- v; this.StateHasChanged()) })
                        ])

                    if show "stepper" then
                        gallerySection "stepper" "Stepper" (html.fragment [
                            card "Starts at min (1)"
                                (fun () -> stepMin <- 1; this.StateHasChanged())
                                (stepperCb { "Value" => stepMin; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMin) (fun v -> stepMin <- v) })
                            card "Starts at mid (5)"
                                (fun () -> stepMid <- 5; this.StateHasChanged())
                                (stepperCb { "Value" => stepMid; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMid) (fun v -> stepMid <- v) })
                            card "Starts at max (9)"
                                (fun () -> stepMax <- 9; this.StateHasChanged())
                                (stepperCb { "Value" => stepMax; "Min" => 1; "Max" => (Some 9 : int option); "OnCommand" => stepCmd (fun () -> stepMax) (fun v -> stepMax <- v) })
                        ])

                    if show "payment-chips" then
                        gallerySection "payment-chips" "PaymentChips" (html.fragment [
                            card "None selected"
                                (fun () -> payNone <- None; this.StateHasChanged())
                                (payment { "SelectedKind" => payNone; "DetailName" => ""; "OnKindCommand" => (fun k -> payNone <- Some k; this.StateHasChanged()); "OnNameCommand" => (fun (_ : string) -> ()) })
                            card "Cash selected"
                                (fun () -> payCash <- Some Cash; this.StateHasChanged())
                                (payment { "SelectedKind" => payCash; "DetailName" => ""; "OnKindCommand" => (fun k -> payCash <- Some k; this.StateHasChanged()); "OnNameCommand" => (fun (_ : string) -> ()) })
                            card "Card selected — with name"
                                (fun () -> payCard <- Some Card; payCardName <- "Business SPARK"; this.StateHasChanged())
                                (payment { "SelectedKind" => payCard; "DetailName" => payCardName; "OnKindCommand" => (fun k -> payCard <- Some k; payCardName <- ""; this.StateHasChanged()); "OnNameCommand" => (fun s -> payCardName <- s; this.StateHasChanged()) })
                            card "App selected — empty name"
                                (fun () -> payApp <- Some App; payAppName <- ""; this.StateHasChanged())
                                (payment { "SelectedKind" => payApp; "DetailName" => payAppName; "OnKindCommand" => (fun k -> payApp <- Some k; payAppName <- ""; this.StateHasChanged()); "OnNameCommand" => (fun s -> payAppName <- s; this.StateHasChanged()) })
                            card "Points selected — empty name"
                                (fun () -> payPoints <- Some Points; payPointsName <- ""; this.StateHasChanged())
                                (payment { "SelectedKind" => payPoints; "DetailName" => payPointsName; "OnKindCommand" => (fun k -> payPoints <- Some k; payPointsName <- ""; this.StateHasChanged()); "OnNameCommand" => (fun s -> payPointsName <- s; this.StateHasChanged()) })
                        ])

                    if show "line-total" then
                        gallerySection "line-total" "LineTotalDisplay" (html.fragment [
                            card "No value — renders nothing"
                                (fun () -> ())
                                (total { "Total" => (None : decimal option) })
                            card "Supplies $4.25"
                                (fun () -> ())
                                (total { "Total" => (Some 4.25m : decimal option) })
                            card "2 × $3.75 = $7.50"
                                (fun () -> ())
                                (total { "Total" => (Some 7.50m : decimal option) })
                            card "9 × $1.00 = $9.00"
                                (fun () -> ())
                                (total { "Total" => (Some 9.00m : decimal option) })
                        ])
                }
            }
        }
