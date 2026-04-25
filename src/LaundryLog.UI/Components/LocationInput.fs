namespace LaundryLog.UI.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Fun.Blazor

type LocationInput() =
    inherit FunComponent()

    [<Parameter>] member val Text: string = ""             with get, set
    [<Parameter>] member val OnTextChanged: string -> unit = ignore with get, set
    [<Inject>]    member val JS: IJSRuntime = Unchecked.defaultof<_> with get, set

    override this.Render() =
        let handleGps _ =
            let gpsScript =
                "new Promise((res,rej)=>navigator.geolocation.getCurrentPosition(" +
                "p=>res(p.coords.latitude.toFixed(4)+', '+p.coords.longitude.toFixed(4)),rej))"
            task {
                try
                    let! coords = this.JS.InvokeAsync<string>("eval", gpsScript)
                    this.OnTextChanged coords
                with _ -> ()
            } |> ignore

        div {
            class' "ll-panel"
            div { class' "ll-panel__title"; "📍 Location" }
            div {
                class' "ll-location-row"
                div {
                    class' "ll-location-input"
                    input {
                        class' "ll-text-input"
                        type' "text"
                        value this.Text
                        placeholder "Laundromat name or address"
                        oninput (fun e -> this.OnTextChanged (string e.Value))
                    }
                    div { class' "ll-location-info"; "GPS checks personal & community data" }
                }
                button {
                    class' "ll-gps-btn"
                    onclick handleGps
                    "📍"
                }
            }
        }
