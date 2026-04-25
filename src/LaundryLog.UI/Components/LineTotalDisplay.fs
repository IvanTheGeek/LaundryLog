namespace LaundryLog.UI.Components

open System.Globalization
open Microsoft.AspNetCore.Components
open Fun.Blazor

type LineTotalDisplay() =
    inherit FunComponent()

    [<Parameter>] member val Total: decimal option = None with get, set

    override this.Render() =
        match this.Total with
        | None -> html.none
        | Some v ->
            div {
                class' "ll-entry-total"
                span { class' "ll-entry-total__label"; "Entry Total" }
                span { class' "ll-entry-total__value"; "$" + v.ToString("F2", CultureInfo.InvariantCulture) }
            }
