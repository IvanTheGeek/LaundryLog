---
id: 001
title: DTCG 2025.10 token files in tokens/ directory
status: accepted
date: 2026-05-02
---

## Context

LaundryLog needed a design token system. Options were:
1. Keep hardcoded CSS custom properties in component stylesheets
2. Use a proprietary token format or JSON-Schema-defined format
3. Adopt DTCG 2025.10 (W3C Design Tokens Community Group spec)

The project already uses `FnTools.DesignTokens` for the CheddarBooks design system.

## Decision

All design tokens live in `tokens/` at the repository root. Token files use `.tokens.json` extension and conform to DTCG 2025.10. A resolver file (`ll.resolver.json`) defines how token sets compose into a resolved token tree.

## Consequences

- Token files are consumable by any DTCG-aware tool (Penpot, Style Dictionary, etc.).
- `FnTools.DesignTokens` handles parsing, validation, and resolution.
- JSONC comments (`//`, `/* */`) work in `.tokens.json` files via `JsonCommentHandling.Skip`.
- Any future token tooling must speak DTCG — no bespoke formats.
