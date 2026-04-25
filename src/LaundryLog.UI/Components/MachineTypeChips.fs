namespace LaundryLog.UI.Components

open Microsoft.AspNetCore.Components
open Fun.Blazor
open LaundryLog.UI

/// Exclusive machine-type selector: Washer · Dryer · Supplies.
///
/// Reads: Selected (MachineType option from EntryFormView)
/// Dispatches: OnCommand of MachineType
///
/// CSS classes (consumer provides styles):
///   .ll-machine-group              — 3-column grid container
///   .ll-machine-chip               — base chip (neutral default state)
///   .ll-machine-chip--washer       — Washer modifier (teal on selection)
///   .ll-machine-chip--dryer        — Dryer modifier (warm orange on selection)
///   .ll-machine-chip--supplies     — Supplies modifier (soft purple on selection)
///   .ll-machine-chip--selected     — active/selected state (subtle tint + colored border)
///   .ll-machine-chip__icon         — emoji icon inside the chip
type MachineTypeChips() =
    inherit FunComponent()

    [<Parameter>]
    member val Selected: MachineType option = None with get, set

    [<Parameter>]
    member val OnCommand: MachineType -> unit = ignore with get, set

    override this.Render() =
        let chip (mt: MachineType) (icon: string) (label: string) (modifier: string) =
            let isSelected = this.Selected = Some mt
            let cls =
                "ll-machine-chip ll-machine-chip--" + modifier +
                (if isSelected then " ll-machine-chip--selected" else "")
            button {
                class' cls
                onclick (fun _ -> this.OnCommand mt)
                span {
                    class' "ll-machine-chip__icon"
                    icon
                }
                label
            }

        div {
            class' "ll-machine-group"
            chip Washer   "🌊" "Washer"   "washer"
            chip Dryer    "🔥" "Dryer"    "dryer"
            chip Supplies "🧴" "Supplies" "supplies"
        }
