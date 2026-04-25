module LaundryLog.UI.Router

open Fun.Blazor
open Fun.Blazor.Router
open LaundryLog.UI.Dev
open LaundryLog.UI.App

/// Root component — delegates routing to Fun.Blazor's Giraffe-style router.
/// Add routes here as new top-level destinations are introduced.
type RootComponent() =
    inherit FunComponent()

    override _.Render() =
        html.route [
            routeCi "/dev" (html.blazor<GalleryComponent>())
            routeCi "/"    (html.blazor<AppComponent>())
        ]
