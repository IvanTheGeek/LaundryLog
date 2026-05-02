---
id: 005
title: Machine type colours are LaundryLog primitives in ll.tokens.json
status: accepted
date: 2026-05-02
---

## Context

LaundryLog has three machine types: washer, dryer, supplies. Each has a visual colour identity (default, subtle, border). These colours are domain-specific — they have no meaning outside of LaundryLog.

Options:
1. Hardcode machine colours in component styles
2. Add machine colours to cb.tokens.json as generic named colours
3. Define machine colours as primitives in ll.tokens.json under `color.machine.*`

## Decision

Machine type colours are primitives in `ll.tokens.json` under `color.machine.{type}.{variant}` (e.g., `color.machine.washer.default`). Values are OKLCH. No semantic alias layer for machine colours — their names are already semantic.

## Consequences

- Machine colours are discoverable and editable in one place.
- Path `color.machine.washer.default` emits CSS var `--color-machine-washer-default`.
- F# binding: `Tokens.Color.Machine.Washer.Default` (all segments already valid identifiers — no N-prefix needed).
- If a future CheddarBooks design system needs machine-type colour conventions, they can be promoted to cb.tokens.json and ll can alias them.
