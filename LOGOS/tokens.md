---
area: Design Tokens
status: current — 2026-05-02
---

# LaundryLog Token System

## Overview

LaundryLog uses **DTCG 2025.10** design tokens authored in `/tokens/`. The token pipeline
(`FnTools.DesignTokens`) parses, resolves, and emits CSS custom properties and typed F#
bindings from these files.

## Files

```
tokens/
  cb.tokens.json        — CheddarBooks base: primitives (colors, spacing, type, shadows, motion)
  ll.tokens.json        — LaundryLog extension: machine-type colors + semantic aliases
  ll.resolver.json      — Resolution order: cb first, then ll (ll aliases win)
  ll.css                — Auto-emitted CSS custom properties (source of truth for the browser)
  Tokens.fs             — Auto-generated typed F# module (compiled into LaundryLog.UI)
```

`ll.css` and `Tokens.fs` are **generated artifacts** — do not edit them by hand.
Re-emit using `FnTools.DesignTokens.Bindings.emit` and `FnTools.DesignTokens.Css.emit`.

## CSS token names

All DTCG-emitted token names follow this pattern:

```
--color-text-primary          (semantic alias, resolved)
--color-neutral-N50           (primitive, N-prefix for numeric scale)
--color-machine-washer-default
--spacing-N1                  (4px)
--spacing-N4                  (16px)
--font-family-body
--font-size-base
--font-weight-semibold
--font-line-height-normal
--font-letter-spacing-wide
--radius-md
--shadow-focus-ring
--duration-fast
--easing-standard
```

The old `--cb-*` and `--ll-*` names are **retired**. They do not exist in the current CSS.

## F# bindings (Tokens.*)

`tokens/Tokens.fs` is compiled into `LaundryLog.UI` as the first file in the project.
Use these bindings in inline styles and interpolated strings:

```fsharp
// In a component's inline style:
style' $"font-family: {Tokens.Font.Family.Body}; ..."
style' $"color: {Tokens.Color.Text.Primary}; background: {Tokens.Color.Surface.Raised};"
style' $"gap: {Tokens.Spacing.N2};"

// In an element's style attribute:
style' $"box-shadow: {Tokens.Shadow.FocusRing};"
```

Token identifiers are **PascalCase**. Numeric-scale segments get N-prefix:
`spacing.N4` → `Tokens.Spacing.N4` → `"var(--spacing-N4)"`.

Full module tree (top-level):
- `Tokens.Color` — `.Text.*`, `.Surface.*`, `.Border.*`, `.Accent.*`, `.Feedback.*`, `.Machine.*`, `.Neutral.*`, `.Amber.*`, etc.
- `Tokens.Spacing` — `.N0` through `.N24`
- `Tokens.Font` — `.Family.*`, `.Size.*`, `.Weight.*`, `.LetterSpacing.*`, `.LineHeight.*`
- `Tokens.Radius` — `.Xs`, `.Sm`, `.Md`, `.Lg`, `.Xl`, `.N2xl`, `.Pill`
- `Tokens.Shadow` — `.Xs`, `.Sm`, `.Md`, `.Lg`, `.Xl`, `.FocusRing`
- `Tokens.Duration` — `.Instant`, `.Fast`, `.Normal`, `.Slow`
- `Tokens.Easing` — `.Standard`, `.Spring`, `.Out`

## CSS loading

`wwwroot/index.html` loads:
1. `css/tokens.css` — DTCG-emitted vars (light mode `:root {}`) + dark mode overrides
2. `css/app.css` — component styles, all referencing DTCG token names

Dark mode is via `data-theme="dark"` on `<html>`. The token vars are overridden in a
`[data-theme="dark"] { }` block at the bottom of `tokens.css`.

## Re-emitting tokens after token file changes

After editing `cb.tokens.json` or `ll.tokens.json`:

```fsharp
// From an F# script:
#r ".../FnTools.DesignTokens.dll"
#r ".../FnTools.DesignTokens.Css.dll"
#r ".../FnTools.DesignTokens.Bindings.dll"
open FnTools.DesignTokens
open FnTools.DesignTokens.Css
open FnTools.DesignTokens.Bindings

let resolverJson = File.ReadAllText "tokens/ll.resolver.json"
let loadFile name = Ok (File.ReadAllText (Path.Combine("tokens", name)))

match Api.importWithResolver loadFile Map.empty resolverJson with
| Error es -> printfn "%A" es
| Ok tokens ->
    File.WriteAllText("tokens/ll.css", Css.emit tokens)
    File.WriteAllText("tokens/Tokens.fs", Bindings.emit "Tokens" tokens)
```

## Key decisions

See `LOGOS/decisions/` for the full ADR list. The most relevant:
- **ADR 001** — DTCG 2025.10 as the token format
- **ADR 002** — Two-tier structure: cb (base) + ll (LaundryLog)
- **ADR 003** — Clean break from --cb-*/--ll-* names
- **ADR 004** — Component tokens in F# code, not in token files
- **ADR 005** — Machine colors as LL primitives in ll.tokens.json
- **ADR 006** — F# bindings are `string` var() constants, not a wrapper type

## Letter-spacing note

DTCG stores letter-spacing as a unitless number (e.g. `0.05`). The emitted CSS var
(`--font-letter-spacing-wide: 0.05`) is unitless. CSS `letter-spacing` requires a unit,
so use `0.05em` directly in stylesheets and inline styles — not `var(--font-letter-spacing-wide)`.
This is a known limitation; the token definition will be revisited.

## Naming

Token naming follows the EightShapes anatomy (category → property/concept → variant → modifier).
Full analysis and ongoing gaps: `FnTools.DesignTokens/LOGOS/naming.md`.

A rename batch was completed 2026-05-02. Key changes consumers should know:

| Old CSS var | New CSS var | F# binding |
|---|---|---|
| `--color-feedback-success` | `--color-feedback-success-default` | `Tokens.Color.Feedback.Success.Default` |
| `--color-feedback-successSubtle` | `--color-feedback-success-subtle` | `Tokens.Color.Feedback.Success.Subtle` |
| `--color-feedback-dangerSubtle` | `--color-feedback-danger-subtle` | `Tokens.Color.Feedback.Danger.Subtle` |
| `--color-feedback-infoSubtle` | `--color-feedback-info-subtle` | `Tokens.Color.Feedback.Info.Subtle` |
| `--shadow-focusRing` | `--shadow-focus-ring` | `Tokens.Shadow.FocusRing` (unchanged) |
| `--font-lineHeight-*` | `--font-line-height-*` | `Tokens.Font.LineHeight.*` (unchanged) |
| `--font-letterSpacing-*` | `--font-letter-spacing-*` | `Tokens.Font.LetterSpacing.*` (unchanged) |

Use `Tokens.*` bindings, not raw CSS var strings, so future renames require only a re-emit.
