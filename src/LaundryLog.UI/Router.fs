module LaundryLog.UI.Router

open System
open Fun.Blazor
open Microsoft.AspNetCore.Components
open LaundryLog.UI.Dev
open LaundryLog.UI.App

/// Root component — reads the current URL and renders either the main
/// app or the component gallery. Subscribes to LocationChanged so
/// navigating between / and /dev re-renders without a full page reload.
type RootComponent() =
    inherit FunComponent()

    [<Inject>]
    member val Nav: NavigationManager = Unchecked.defaultof<_> with get, set

    override this.OnInitialized() =
        this.Nav.LocationChanged.Add(fun _ -> this.StateHasChanged())

    override this.Render() =
        let path = Uri(this.Nav.Uri).AbsolutePath
        if path.StartsWith("/dev") then
            html.blazor<GalleryComponent>()
        else
            html.blazor<AppComponent>()
