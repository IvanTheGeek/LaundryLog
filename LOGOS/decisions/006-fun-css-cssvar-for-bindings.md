---
id: 006
title: Fun.Css CssVar as the typed binding type for token references in F# components
status: accepted
date: 2026-05-02
---

## Context

LaundryLog UI is built with Fun.Blazor (Fun.Css for styling). Token references in component code need to be type-checked at compile time. Options:

1. Plain string CSS var names (`"var(--color-text-primary)"`)
2. Generated constants (static `string` values in a `Tokens` module)
3. `Fun.Css.CssVar` — a typed wrapper that generates `var(--name)` for use in Fun.Css builders

## Decision

The `FnTools.DesignTokens.Bindings` emitter generates a `Tokens` module where each leaf token is a `string` constant holding the CSS `var()` reference (e.g. `"var(--color-text-primary)"`). Components reference tokens as `Tokens.Color.Text.Primary` — the compiler enforces valid token paths.

`Fun.Css.CssVar` was the original design target, but `Fun.Css.CssBuilder` property operations (e.g. `color`) accept plain strings directly, so no wrapper type is needed. The generated file has zero runtime dependencies.

## Consequences

- Token references in components are compile-time checked: invalid paths fail at build.
- Renaming a token propagates as a compile error to all consumers.
- String values work with any F# CSS approach — Fun.Css, inline styles, or attribute helpers.
- Generated bindings are re-emitted whenever token files change; they are not hand-maintained.
- `tokens/Tokens.fs` in LaundryLog is the current generated output (161 lines).
