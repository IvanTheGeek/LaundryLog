---
id: 004
title: Component-level tokens live in code, not in token files
status: accepted
date: 2026-05-02
---

## Context

The three-tier token model: primitive → semantic → component. Component tokens (e.g., `button.primary.background`) map semantic tokens to specific component slots.

Options:
1. Define component tokens in `.tokens.json` files
2. Define component tokens directly in F# component code as `CssVar` references

## Decision

Component tokens are expressed in code (Fun.Blazor / Fun.Css components) by referencing semantic `CssVar` values directly. No component token layer in `.tokens.json` files.

## Consequences

- Token files stay at primitive and semantic tier — no explosion of per-component entries.
- Component token semantics are visible at the call site in F#, not buried in JSON.
- Refactoring a component's token usage is an F# edit with compile-time checking.
- If a component's token mapping is shared across multiple components, it can be extracted into a shared F# module — this is the natural abstraction point.
- This decision can be revisited if Penpot integration requires component token annotations in the file.
