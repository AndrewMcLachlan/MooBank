# Design — Balance adjustment from the Add Transaction form

**Date:** 2026-07-12
**Status:** Approved design, pending spec review → implementation plan.

## Goal

On manual and virtual accounts, let the user set a **new balance** (instead of a transaction
amount) directly from the Add Transaction dialog. The Add Transaction form gains a second,
mutually-exclusive input — **New Balance** — and whichever of *Amount* / *New Balance* is filled
determines which endpoint is called. A new-balance submission records a balance-adjustment
transaction (`TransactionSubType.BalanceAdjustment`, amount = `newBalance − currentBalance`) via the
existing `/balance-adjustment` endpoint.

## Existing building blocks (reused, not rebuilt)

- **Command/endpoint:** `Modules.Transactions/Commands/UpdateBalance.cs` → `POST /balance-adjustment`
  (`setBalance` mutation). Takes a `CreateTransaction` body, computes `Amount − currentBalance`, and
  writes a `BalanceAdjustment` transaction.
- **Hooks:** `hooks/useUpdateBalance.ts` (balance path) and `routes/accounts/-hooks/useCreateTransaction.ts`
  (normal path).
- **Form:** `routes/accounts/-transactions/components/AddTransaction.tsx` already collects Amount /
  Date / Description / Reference. Its `balanceUpdate` prop mode is **dead** (never passed `true`) and
  is replaced by this design.

## Scope

Frontend change plus a one-line-plus backend change. No new command, endpoint, or DTO; the
`/balance-adjustment` body already carries the full `CreateTransaction`, so **no OpenAPI/client
regeneration**.

## Backend

`Modules.Transactions/Commands/UpdateBalance.cs`:

1. After `Transaction.Create(...)`, set the reference so it is persisted, mirroring `Create.cs`:
   `transaction.Reference = command.BalanceUpdate.Reference;`
   `Description` already flows via `command.BalanceUpdate.Description ?? "Balance adjustment"`. Both
   remain optional (the `CreateTransaction` fields are nullable).
2. Switch the handler's `IUnitOfWork` to `IAuditingUnitOfWork` and save via the auditing overload
   (as `Create.cs` does), so the adjustment is audited like other user mutations (project standard).

No change to `Transaction.Create` (it takes no reference; the codebase sets `.Reference` as a
property post-construction).

## Frontend

`routes/accounts/-transactions/components/AddTransaction.tsx`:

- **New Balance input** rendered when `account.controller` is `"Manual"` or `"Virtual"`. (Import has
  no Add button, so the modal never opens there.)
- **Form model:** the RHF form type is `CreateTransaction & { newBalance?: string }`. `newBalance` is
  form-only and never sent verbatim.
- **Disable-the-other:** watch both fields. When one holds a **non-empty string**, disable and clear
  the other. The non-empty-string test (not truthiness) is required so a new balance of `0` counts as
  filled.
- **Submit routing:**
  - `newBalance` non-empty → `updateBalance.mutateAsync(account.id, { ...transaction, amount: Number(newBalance) })`.
  - else → `addTransaction.mutateAsync(account.id, transaction)`.
  In both cases strip `newBalance` from the payload.
- **Validation:** exactly one of Amount / New Balance non-empty. If neither is filled, block submit
  with "Enter an amount or a new balance."
- **Shared fields:** Date, Description, Reference stay for both paths. Default Date to today.
- **Cleanup:** remove the dead `balanceUpdate` prop and its title/submit branches; the modal title is
  simply "Add Transaction".

`routes/accounts/-transactions/Transactions.tsx`:

- Drop the now-unused `balanceUpdate={false}` prop on `<AddTransaction>`. The existing Manual/Virtual
  "Add" button is the trigger; no new button.

## Data flow

```
Add Transaction dialog (Manual/Virtual account)
  ├─ Amount filled      → useCreateTransaction (Create command)       → normal transaction
  └─ New Balance filled → useUpdateBalance (setBalance / UpdateBalance) → BalanceAdjustment transaction
                             (amount = newBalance − currentBalance; description + reference carried)
```

## Testing

Vitest component test on `AddTransaction`:

- New Balance filled → calls the balance hook with `amount = Number(newBalance)`; create hook not called.
- Amount filled → calls the create hook; balance hook not called.
- Typing in one clears + disables the other (both directions); `0` in New Balance is treated as filled.
- Both empty → submit blocked, validation message shown.
- New Balance input renders for `Manual` and `Virtual`, absent otherwise.

Backend: existing `UpdateBalanceHandler` tests (if any) extended to assert `Reference` is persisted;
otherwise a focused handler test covering description + reference passthrough.

## Non-goals

- No changes to the separate `Modules.Instruments/Commands/VirtualInstruments/UpdateBalance.cs`
  (virtual-account management path); this form uses the Transactions `/balance-adjustment` endpoint
  uniformly for both Manual and Virtual.
- No new endpoint, DTO, or OpenAPI/client regeneration.
- No change to `Transaction.Create` factory signatures.

## Verification

- `dotnet build` + backend tests green; `npm run build` + `npm run lint` + `npm test` green.
- Manual: on a Manual and a Virtual account, set a new balance → a `BalanceAdjustment` transaction
  appears with the entered description/reference and the balance moves to the entered value; adding a
  normal amount still works and is unaffected; filling one input disables the other.
