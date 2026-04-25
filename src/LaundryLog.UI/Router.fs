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
            // /dev/section-id — GalleryComponent reads the URL itself via NavigationManager
            routeCif "/dev/%s" (fun _ -> html.blazor<GalleryComponent>())
            routeCi  "/dev"    (html.blazor<GalleryComponent>())
            routeCi  "/"       (html.blazor<AppComponent>())
        ]
