module LaundryLog.UI.App

open Fun.Blazor

// Minimal POC component — proves Fun.Blazor CE syntax and reactive state work
// on .NET 10 Blazor WASM. Mirrors the LaundryLog stepper interaction pattern.

type AppComponent() =
    inherit FunComponent()

    let mutable count = 0

    override _.Render() =
        div {
            style' "font-family: var(--cb-font-body, system-ui); max-width: 360px; margin: 2rem auto; padding: 1.5rem; background: var(--cb-surface-base, #f9f7f4);"
            h1 {
                style' "color: var(--cb-text-accent, #7a4f1e); font-size: var(--cb-text-2xl, 1.9rem); margin-bottom: 0.5rem;"
                "LaundryLog"
            }
            p {
                style' "color: var(--cb-text-secondary, #6b5c4a); margin-bottom: 2rem;"
                "Fun.Blazor · .NET 10 · POC"
            }

            // Stepper — mirrors the LaundryLog quantity control
            div {
                style' "display: flex; align-items: center; gap: 1rem;"
                button {
                    style' "width: 64px; height: 64px; border-radius: 50%; background: var(--cb-accent, #d4820a); color: white; border: none; font-size: 1.5rem; cursor: pointer;"
                    onclick (fun _ -> if count > 0 then count <- count - 1)
                    "−"
                }
                span {
                    style' "font-size: var(--cb-text-3xl, 2.4rem); font-weight: var(--cb-weight-bold, 700); min-width: 3rem; text-align: center;"
                    string count
                }
                button {
                    style' "width: 64px; height: 64px; border-radius: 50%; background: var(--cb-accent, #d4820a); color: white; border: none; font-size: 1.5rem; cursor: pointer;"
                    onclick (fun _ -> count <- count + 1)
                    "+"
                }
            }

            p {
                style' "margin-top: 1.5rem; color: var(--cb-text-muted, #999); font-size: var(--cb-text-sm, 0.8rem);"
                "Click +/− to verify reactive state ↑"
            }
        }
