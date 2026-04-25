module LaundryLog.UI.Router

open Fun.Blazor
open Fun.Blazor.Operators
open Fun.Blazor.Router
open LaundryLog.UI.Dev
open LaundryLog.UI.App

/// Root component — delegates routing to Fun.Blazor's Giraffe-style router.
type RootComponent() =
    inherit FunComponent()

    let gallery = ComponentBuilder<GalleryComponent>()

    override _.Render() =
        html.route [
            // /dev/location-input, /dev/machine-chips, etc. — isolated story view
            routeCif "/dev/%s" (fun story -> gallery { "ActiveStory" => story })
            // /dev — full gallery overview
            routeCi "/dev" (html.blazor<GalleryComponent>())
            routeCi "/"   (html.blazor<AppComponent>())
        ]
