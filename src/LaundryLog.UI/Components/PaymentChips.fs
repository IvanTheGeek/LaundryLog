namespace LaundryLog.UI.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Fun.Blazor
open LaundryLog.UI

type PaymentChips() =
    inherit FunComponent()

    [<Parameter>] member val SelectedKind: PaymentKind option = None with get, set
    [<Parameter>] member val DetailName: string = ""          with get, set
    [<Parameter>] member val OnKindCommand: PaymentKind -> unit       = ignore with get, set
    [<Parameter>] member val OnNameCommand: string -> unit             = ignore with get, set
    [<Inject>]    member val JS: IJSRuntime = Unchecked.defaultof<_>  with get, set

    override this.Render() =
        let chip (mt: PaymentKind) (icon: string) (label: string) =
            let cls =
                "ll-payment-chip" +
                (if this.SelectedKind = Some mt then " ll-payment-chip--selected" else "")
            button {
                class' cls
                onclick (fun _ -> this.OnKindCommand mt)
                span { class' "ll-payment-chip__icon"; icon }
                label
            }

        let nameHint =
            match this.SelectedKind with
            | Some Card   -> "Card name (e.g. Business SPARK)"
            | Some App    -> "App name (e.g. LaundryPay)"
            | Some Points -> "Program name (e.g. Love's Rewards)"
            | _           -> ""

        div {
            style' $"display:flex;flex-direction:column;gap:{Tokens.Spacing.N2};"
            div {
                class' "ll-payment-group"
                chip Cash   "💵" "Cash"
                chip Card   "💳" "Card"
                chip App    "📱" "App"
                chip Points "⭐" "Points"
            }
            match this.SelectedKind with
            | None | Some Cash -> ()
            | _ ->
                input {
                    class' "ll-text-input"
                    type' "text"
                    value this.DetailName
                    placeholder nameHint
                    onfocus (fun _ -> this.JS.InvokeVoidAsync("eval", "document.activeElement.select()") |> ignore)
                    oninput (fun e -> this.OnNameCommand (string e.Value))
                }
        }
