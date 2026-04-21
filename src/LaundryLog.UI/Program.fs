module LaundryLog.UI.Program

open Microsoft.AspNetCore.Components.WebAssembly.Hosting

[<EntryPoint>]
let main args =
    let builder = WebAssemblyHostBuilder.CreateDefault(args)
    builder.RootComponents.Add<App.AppComponent>("#app")
    builder.Build().RunAsync() |> ignore
    0
