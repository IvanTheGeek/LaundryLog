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

The `FnTools.DesignTokens.Bindings` emitter (planned) generates a `Tokens` module where each leaf token is a `Fun.Css.CssVar` value. Components reference tokens as `Tokens.Color.Text.Primary` — the type system ensures only valid token paths appear in component code.

## Consequences

- Token references in components are compile-time checked.
- Renaming a token propagates as a compile error to all consumers.
- `Fun.Css.CssVar` values are only useful with Fun.Css — this ties the generated bindings to the Fun.Css ecosystem.
- The bindings emitter takes a `Fun.Css` package dependency; the core `FnTools.DesignTokens.*` packages do not.
- Generated bindings are re-emitted whenever token files change; they are not hand-maintained.
