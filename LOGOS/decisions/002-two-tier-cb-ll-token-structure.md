---
id: 002
title: Two-tier token structure — cb (CheddarBooks base) + ll (LaundryLog-specific)
status: accepted
date: 2026-05-02
---

## Context

LaundryLog is built on CheddarBooks (cb), a shared design system that also underpins other FnTools projects. LaundryLog needs both the shared primitives and its own domain-specific tokens (machine type colours, etc.).

Two structures were considered:
1. Single flat token file mixing cb and ll tokens
2. Two separate files: `cb.tokens.json` (base) and `ll.tokens.json` (overrides/additions)

## Decision

Two files, composed by the resolver:

- `cb.tokens.json` — CheddarBooks primitives and semantic aliases; lives here as a copy (authoritative source is the CheddarBooks tokens project once that exists).
- `ll.tokens.json` — LaundryLog-specific tokens only; semantic tokens alias into `cb.*` where appropriate.
- `ll.resolver.json` — resolution order `["cb", "ll"]`; later sets win on conflict.

## Consequences

- cb tokens are upgraded by updating `cb.tokens.json` and re-emitting.
- ll tokens only define what is unique to LaundryLog — no duplication of cb values.
- When a CheddarBooks token project exists (FnTools.FnHCI or similar), `cb.tokens.json` becomes a generated artifact from that project.
- The resolver order (`cb` then `ll`) means ll aliases resolve into the final cb values.
