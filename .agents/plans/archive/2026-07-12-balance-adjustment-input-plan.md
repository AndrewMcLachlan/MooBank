# Balance Adjustment via Add Transaction — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a user set a new balance (instead of a transaction amount) from the Add Transaction dialog on Manual and Virtual accounts; the filled input picks the endpoint, and a new-balance submission records a `BalanceAdjustment` transaction carrying optional description + reference.

**Architecture:** Frontend-led. The `/balance-adjustment` command/endpoint and both React Query hooks already exist. Backend change is limited to persisting the reference and auditing the save in the existing `UpdateBalanceHandler`. Frontend adds a second, mutually-exclusive "New Balance" input to `AddTransaction` and routes submit by which field is filled.

**Tech Stack:** .NET 10 / C# (xUnit v3 + Moq for backend tests), React 19 + TypeScript + React Hook Form + moo-ds (frontend, no unit-test infra — verified via `npm run build` typecheck + `npm run lint` + manual).

## Global Constraints

- Backend commands return the full DTO of the created resource (house style).
- User-initiated mutations save via `IAuditingUnitOfWork` (project standard).
- `String.` (framework type) for static string calls, not `string.`.
- Frontend constants are camelCase; no SCREAMING_SNAKE_CASE.
- No new endpoint, DTO, or OpenAPI/client regeneration — the `/balance-adjustment` body already carries the full `CreateTransaction`.
- Design reference: `.agents/plans/2026-07-12-balance-adjustment-input-design.md`.

---

### Task 1: Persist reference and audit the balance adjustment (backend)

**Files:**
- Modify: `src/MooBank.Modules.Transactions/Commands/UpdateBalance.cs`
- Test: `tests/MooBank.Modules.Transactions.Tests/Commands/UpdateBalanceTests.cs`

**Interfaces:**
- Consumes: `TestMocks.AuditingUnitOfWorkMock` (`Mock<IAuditingUnitOfWork>`, already set up), `TestEntities.CreateTransactionInstrument(id, balance)`, `CreateTransaction(decimal Amount, string Description, string? Reference, DateTimeOffset TransactionTime)`.
- Produces: `UpdateBalanceHandler(IInstrumentRepository, ITransactionRepository, IUserIdProvider, IAuditingUnitOfWork)` — note the 4th constructor parameter changes from `IUnitOfWork` to `IAuditingUnitOfWork`. The created transaction's `Reference` equals `command.BalanceUpdate.Reference`; the save calls `SaveChangesAsync("Adjusted Balance", "Transaction", transaction.Id, ct)`.

- [ ] **Step 1: Add a failing test for reference passthrough**

Add this test to `UpdateBalanceTests.cs` (note it constructs the handler with `AuditingUnitOfWorkMock` — the existing tests will be updated in Step 4):

```csharp
    [Fact]
    public async Task Handle_WithReference_PersistsReference()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, "Adj", "REF-123", DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("REF-123", capturedTransaction.Reference);
    }
```

- [ ] **Step 2: Run it to verify it fails to compile / fail**

Run: `dotnet test tests/MooBank.Modules.Transactions.Tests --filter "FullyQualifiedName~UpdateBalanceTests.Handle_WithReference_PersistsReference"`
Expected: FAILS to build — `UpdateBalanceHandler` does not accept `IAuditingUnitOfWork` yet (its 4th param is `IUnitOfWork`).

- [ ] **Step 3: Update the handler to persist the reference and audit the save**

Replace the whole file `src/MooBank.Modules.Transactions/Commands/UpdateBalance.cs` with:

```csharp
using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateBalance(Guid InstrumentId, CreateTransaction BalanceUpdate) : ICommand<MooBank.Models.Transaction>;

internal class UpdateBalanceHandler(IInstrumentRepository instrumentRepository, ITransactionRepository transactionRepository, IUserIdProvider userIdProvider, IAuditingUnitOfWork unitOfWork) : ICommandHandler<UpdateBalance, MooBank.Models.Transaction>
{
    public async ValueTask<MooBank.Models.Transaction> Handle(UpdateBalance command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, cancellationToken);

        if (instrument is not Domain.Entities.Instrument.TransactionInstrument transactionInstrument)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        var amount = command.BalanceUpdate.Amount - transactionInstrument.Balance;

        var transaction = Domain.Entities.Transactions.Transaction.Create(
            transactionInstrument,
            userIdProvider.CurrentUserId,
            amount,
            command.BalanceUpdate.Description ?? "Balance adjustment",
            command.BalanceUpdate.TransactionTime.DateTime,
            TransactionSubType.BalanceAdjustment,
            "Web"
        );

        transaction.Reference = command.BalanceUpdate.Reference;

        transactionRepository.Add(transaction);

        await unitOfWork.SaveChangesAsync("Adjusted Balance", "Transaction", transaction.Id, cancellationToken);

        return transaction.ToModel();
    }
}
```

- [ ] **Step 4: Point the existing tests at the auditing unit of work**

In `UpdateBalanceTests.cs`, every handler construction currently passes `_mocks.UnitOfWorkMock.Object` as the 4th argument. Change all of them to `_mocks.AuditingUnitOfWorkMock.Object`. Then replace the body of `Handle_ValidCommand_SavesChanges`'s assertion so it verifies the auditing overload:

```csharp
        // Assert
        _mocks.AuditingUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync("Adjusted Balance", "Transaction", It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Once);
```

- [ ] **Step 5: Run the full test file to verify all pass**

Run: `dotnet test tests/MooBank.Modules.Transactions.Tests --filter "FullyQualifiedName~UpdateBalanceTests"`
Expected: PASS — all tests in `UpdateBalanceTests` green (including the new reference test and the reworked save-verification).

- [ ] **Step 6: Commit**

```bash
git add src/MooBank.Modules.Transactions/Commands/UpdateBalance.cs tests/MooBank.Modules.Transactions.Tests/Commands/UpdateBalanceTests.cs
git commit -m "feat(transactions): carry reference and audit the balance adjustment"
```

---

### Task 2: New Balance input in Add Transaction (frontend)

**Files:**
- Modify: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/AddTransaction.tsx`
- Modify: `src/MooBank.Web.App/src/routes/accounts/-transactions/Transactions.tsx:56`

**Interfaces:**
- Consumes: `useAccount()` → `{ id: string, controller: "Manual" | "Virtual" | "Import" }`; `useUpdateBalance()` → `{ mutateAsync(accountId, CreateTransaction), isPending }`; `useCreateTransaction()` → `{ mutateAsync(accountId, CreateTransaction), isPending }`; `CreateTransaction = { amount: number; description: string; reference?: string; transactionTime: string }`.
- Produces: `AddTransaction` component whose props are `{ show, onClose, onSave? }` — the `balanceUpdate` prop is removed.

**No unit test:** the web app has no test runner configured (no `test` script, no Vitest/RTL). Verify with `npm run build` (tsgo typecheck) + `npm run lint` + manual.

- [ ] **Step 1: Rewrite AddTransaction with the mutually-exclusive New Balance input**

Replace the whole file `src/MooBank.Web.App/src/routes/accounts/-transactions/components/AddTransaction.tsx` with:

```tsx
import React from "react";
import { Button, Modal, Form } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { format } from "date-fns/format";

import { useAccount } from "components";
import type { CreateTransaction } from "models/transactions";
import { useUpdateBalance } from "hooks/useUpdateBalance";
import { useCreateTransaction } from "routes/accounts/-hooks/useCreateTransaction";
import { CurrencyInput } from "components";

type AddTransactionForm = Omit<CreateTransaction, "amount"> & {
    amount?: number | "";
    newBalance?: number | "";
};

// A number field is "filled" only when it holds a real value. Guards against "" (empty),
// undefined/null, and NaN (an empty native number input), while treating 0 as filled so a
// new balance of zero is accepted.
const isFilled = (value: unknown): boolean =>
    value !== undefined && value !== null && String(value).trim() !== "" && String(value) !== "NaN";

export const AddTransaction: React.FC<AddTransactionProps> = ({ show, onClose, onSave }) => {

    const account = useAccount();

    const addTransaction = useCreateTransaction();
    const updateBalance = useUpdateBalance();

    const isPending = addTransaction.isPending || updateBalance.isPending;

    const allowBalance = account?.controller === "Manual" || account?.controller === "Virtual";

    const form = useForm<AddTransactionForm>({
        defaultValues: {
            amount: "",
            newBalance: "",
            description: "",
            reference: "",
            transactionTime: format(new Date(), "yyyy-MM-dd"),
        },
    });

    const amountFilled = isFilled(form.watch("amount"));
    const newBalanceFilled = allowBalance && isFilled(form.watch("newBalance"));

    const handleSubmit = (values: AddTransactionForm) => {
        if (!account) return;

        if (newBalanceFilled) {
            updateBalance.mutateAsync(account.id, {
                amount: Number(values.newBalance),
                description: values.description,
                reference: values.reference,
                transactionTime: values.transactionTime,
            });
        } else {
            addTransaction.mutateAsync(account.id, {
                amount: Number(values.amount),
                description: values.description,
                reference: values.reference,
                transactionTime: values.transactionTime,
            });
        }

        onSave?.();
    };

    if (!account) return null;

    return (
        <Modal show={show} onHide={() => onClose()} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Add Transaction</Modal.Title>
            </Modal.Header>
            <Form form={form} onSubmit={handleSubmit} layout="horizontal">
                <Modal.Body>
                    <Form.Group groupId="amount">
                        <Form.Label>Amount</Form.Label>
                        <CurrencyInput disabled={newBalanceFilled} />
                    </Form.Group>
                    {allowBalance && (
                        <Form.Group groupId="newBalance">
                            <Form.Label>New Balance</Form.Label>
                            <CurrencyInput disabled={amountFilled} />
                        </Form.Group>
                    )}
                    <Form.Group groupId="transactionTime">
                        <Form.Label>Date</Form.Label>
                        <Form.Input type="date" required />
                    </Form.Group>
                    <Form.Group groupId="description">
                        <Form.Label>Description</Form.Label>
                        <Form.TextArea maxLength={255} />
                    </Form.Group>
                    <Form.Group groupId="reference">
                        <Form.Label>Reference</Form.Label>
                        <Form.Input type="text" maxLength={150} />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={() => onClose()}>Close</Button>
                    <Button variant="primary" type="submit" disabled={isPending || (!amountFilled && !newBalanceFilled)}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}

export interface AddTransactionProps {
    show: boolean;
    onClose: () => void;
    onSave?: () => void;
}
```

- [ ] **Step 2: Drop the dead `balanceUpdate` prop at the call site**

In `src/MooBank.Web.App/src/routes/accounts/-transactions/Transactions.tsx`, line 56 currently reads:

```tsx
            <AddTransaction show={show} onClose={() => setShow(false)} onSave={() => setShow(false)} balanceUpdate={false} />
```

Change it to (remove `balanceUpdate={false}`):

```tsx
            <AddTransaction show={show} onClose={() => setShow(false)} onSave={() => setShow(false)} />
```

- [ ] **Step 3: Typecheck**

Run: `cd src/MooBank.Web.App && npm run build`
Expected: builds with no TypeScript errors.

- [ ] **Step 4: Lint**

Run: `cd src/MooBank.Web.App && npm run lint`
Expected: 0 errors (warnings acceptable per project baseline).

- [ ] **Step 5: Manual verification**

Run the app (restart the API + Vite). On a **Manual** account and a **Virtual** account:
- The Add dialog shows both **Amount** and **New Balance**; typing in one disables the other.
- Enter a **New Balance** + optional description/reference → a `Balance Adjustment` transaction appears, the balance moves to the entered value, and the description/reference are recorded.
- Enter an **Amount** (New Balance empty) → a normal transaction is added, unchanged from before.
- With both empty, **Save** is disabled.

- [ ] **Step 6: Commit**

```bash
git add src/MooBank.Web.App/src/routes/accounts/-transactions/components/AddTransaction.tsx src/MooBank.Web.App/src/routes/accounts/-transactions/Transactions.tsx
git commit -m "feat(transactions): add New Balance input to Add Transaction for manual/virtual accounts"
```

---

## Notes

- **No frontend automated test.** The web app has no test runner wired up (no `test` script, no Vitest/RTL/jsdom). Standing that up is out of scope for this feature; the frontend change is covered by typecheck + lint + the manual checklist. If you want, a follow-up can add Vitest + React Testing Library and the `AddTransaction` behaviour tests described in the design doc.
- **Validation nuance:** the design mentioned an "Enter an amount or a new balance" message. This plan enforces the same rule more simply by disabling **Save** while both inputs are empty (paired with disable-the-other for the exclusivity). No error text is shown.
