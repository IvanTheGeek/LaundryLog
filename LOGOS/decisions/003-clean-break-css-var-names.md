---
id: 003
title: Clean break from --cb-* / --ll-* CSS custom property naming
status: accepted
date: 2026-05-02
---

## Context

The original LaundryLog design system used prefixed CSS custom properties: `--cb-color-neutral-50`, `--ll-color-machine-washer`. The DTCG token path structure emits `--color-neutral-N50`, `--color-machine-washer-default`.

Options:
1. Preserve old names via an alias mapping layer
2. Emit a compatibility shim alongside the new vars
3. Clean break — update all component uses in one pass

## Decision

Clean break. The old `--cb-*` and `--ll-*` names are replaced with the DTCG-derived form (`--color-*`, `--spacing-*`, etc.) in one migration pass. No shim or alias layer.

## Consequences

- All component stylesheets and inline styles that reference the old names must be updated.
- The DTCG-derived names are the single authoritative surface — no two names for the same token.
- Any Penpot design files using the old variable names need to be updated when Penpot integration goes live.
- Numeric scale segments use the N-prefix convention: `--color-neutral-N500` (matching the F# emitter's `N-prefix` rule from FnTools.DesignTokens ADR 010).
