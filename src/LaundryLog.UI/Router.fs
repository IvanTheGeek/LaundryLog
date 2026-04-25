module LaundryLog.UI.Router

open Fun.Blazor
open Fun.Blazor.Router
open LaundryLog.UI.Dev
open LaundryLog.UI.App

/// Root component — delegates routing to Fun.Blazor's Giraffe-style router.
type RootComponent() =
    inherit FunComponent()

    override _.Render() =
        html.route [
            // GalleryComponent owns its own sub-navigation (/dev/location-input etc.)
            // by reading NavigationManager.Uri directly. No routeCif needed here.
            routeCi "/dev" (html.blazor<GalleryComponent>())
            routeCi "/"    (html.blazor<AppComponent>())
        ]
