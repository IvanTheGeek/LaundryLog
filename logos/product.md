---
area: Product
status: reviewed
---

# LaundryLog Product

## What It Is

LaundryLog is an expense-capture tool for recording laundry costs on the road.
It is not a laundry management app. It is a lightweight, fast expense journal.

The core problem: receipts are often missing or inconvenient at laundromats and
truck stops. LaundryLog replaces them with a quick on-device log that satisfies
IRS recordkeeping requirements.

## Target Users

- Truck drivers and professional drivers logging laundry as a business expense
- Travelers doing laundry away from home
- Anyone needing a verifiable, auditable expense trail without paper receipts

## Product Goals

- Expense entry should be fast on a phone — location, machine type, quantity, price, payment
- Missing receipts are not a problem — the log itself is the record
- Enough detail to support tax deduction documentation and later reconciliation
- User owns the data — no cloud dependency required for the app to be useful

## Delivery Direction

- Browser-first, mobile-friendly, installable as PWA
- First useful product does not require constant server contact
- Offline entry at the laundromat is a first-class requirement, not an afterthought
- Optional hosted services (location hints, sync) enrich the app but do not define whether it works

## Privacy and Ownership

- Expense data remains the user's — no routine sharing with advertising systems or platforms
- Local-first: the app should stand on its own without cloud contact
- Offline entry with later convergence — local ownership is preserved across sync
- Outbound sharing is explicit and user-directed
- Hosted features are additive, not load-bearing

## Deferred Delivery Decisions

- Exact offline storage shape (OPFS, IndexedDB, or file-based)
- Exact sync/convergence boundary
- Native host path (if any beyond PWA)
- Hosted-service boundary for shared location knowledge
