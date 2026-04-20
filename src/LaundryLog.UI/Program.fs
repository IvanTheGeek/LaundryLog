module LaundryLog.UI.Program

open System
open Microsoft.AspNetCore.Components.WebAssembly.Hosting

[<EntryPoint>]
let main args =
    let builder = WebAssemblyHostBuilder.CreateDefault(args)
    builder.RootComponents.Add<App.AppComponent>("#app")
    builder
        .Build()
        .RunAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously
    0
