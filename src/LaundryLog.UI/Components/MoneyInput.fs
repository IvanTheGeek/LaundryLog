namespace LaundryLog.UI.Components

open System
open System.Globalization
open Microsoft.AspNetCore.Components
open Fun.Blazor

/// Price/amount entry: coin-decrement · $ text field · coin-increment.
/// Step = $0.25 (one quarter). Value is None when the field is empty or invalid.
///
/// Reads: Value (decimal option)
/// Dispatches: OnCommand (decimal option — None to clear)
///
/// CSS classes:
///   .ll-money-grid       — 3-column grid: coin | field | coin
///   .ll-coin-btn         — metallic quarter-coin button
///   .ll-coin-btn__sign   — ± symbol inside the coin
///   .ll-coin-btn__label  — "25¢" label inside the coin
///   .ll-money-field-wrap — flex row: $ symbol + input
///   .ll-money-currency   — "$" prefix
///   .ll-money-input      — price text field
type MoneyInput() =
    inherit FunComponent()

    [<Parameter>]
    member val Value: decimal option = None with get, set

    [<Parameter>]
    member val OnCommand: decimal option -> unit = ignore with get, set

    override this.Render() =
        let displayValue =
            match this.Value with
            | Some v -> v.ToString("F2", CultureInfo.InvariantCulture)
            | None -> ""

        let coinDown _ =
            let next =
                match this.Value with
                | None -> None
                | Some v ->
                    let n = v - 0.25m
                    if n > 0m then Some n else None
            this.OnCommand next

        let coinUp _ =
            let next =
                match this.Value with
                | None -> Some 0.25m
                | Some v -> Some (v + 0.25m)
            this.OnCommand next

        let handleChange (e: ChangeEventArgs) =
            let s = string e.Value
            if s = "" then this.OnCommand None
            else
                match Decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) with
                | true, v when v > 0m -> this.OnCommand (Some v)
                | _ -> ()

        div {
            class' "ll-money-grid"
            button {
                class' "ll-coin-btn"
                onclick coinDown
                span { class' "ll-coin-btn__sign"; "−" }
                span { class' "ll-coin-btn__label"; "25¢" }
            }
            div {
                class' "ll-money-field-wrap"
                span { class' "ll-money-currency"; "$" }
                input {
                    class' "ll-money-input"
                    type' "text"
                    value displayValue
                    placeholder "0.00"
                    onchange handleChange
                }
            }
            button {
                class' "ll-coin-btn"
                onclick coinUp
                span { class' "ll-coin-btn__sign"; "+" }
                span { class' "ll-coin-btn__label"; "25¢" }
            }
        }
