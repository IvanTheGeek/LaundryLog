namespace LaundryLog.UI.Components

open Microsoft.AspNetCore.Components
open Fun.Blazor
open LaundryLog.UI

/// Exclusive machine-type selector: Washer · Dryer · Supplies.
///
/// Reads: Selected (MachineType option from EntryFormView)
/// Dispatches: MachineTypeSelected of MachineType
///
/// CSS classes (consumer provides styles):
///   .fn-machine-chips              — flex row container
///   .fn-machine-chip               — base chip style
///   .fn-machine-chip--washer       — Washer colour modifier
///   .fn-machine-chip--dryer        — Dryer colour modifier
///   .fn-machine-chip--supplies     — Supplies colour modifier
///   .fn-machine-chip--selected     — active/selected state
type MachineTypeChips() =
    inherit FunComponent()

    [<Parameter>]
    member val Selected: MachineType option = None with get, set

    [<Parameter>]
    member val OnCommand: MachineType -> unit = ignore with get, set

    override this.Render() =
        let chip (mt: MachineType) (label: string) (modifier: string) =
            let isSelected = this.Selected = Some mt
            let cls =
                "fn-machine-chip fn-machine-chip--" + modifier +
                (if isSelected then " fn-machine-chip--selected" else "")
            button {
                class' cls
                onclick (fun _ -> this.OnCommand mt)
                label
            }

        div {
            class' "fn-machine-chips"
            chip Washer   "Washer"   "washer"
            chip Dryer    "Dryer"    "dryer"
            chip Supplies "Supplies" "supplies"
        }
