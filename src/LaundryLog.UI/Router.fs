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
            // /dev — full gallery; ActiveStory must be explicitly "" so Blazor resets
            // it when reusing the component instance from a prior story route
            routeCi "/dev" (gallery { "ActiveStory" => "" })
            routeCi "/"    (html.blazor<AppComponent>())
        ]
