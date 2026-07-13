import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { LogicalAccount } from "api/types.gen";

// Stable spies shared with the mocked hook modules (hoisted above the vi.mock calls).
const mocks = vi.hoisted(() => ({
    createMutate: vi.fn(),
    balanceMutate: vi.fn(),
}));

vi.mock("routes/accounts/-hooks/useCreateTransaction", () => ({
    useCreateTransaction: () => ({ mutateAsync: mocks.createMutate, isPending: false }),
}));
vi.mock("routes/accounts/-hooks/useUpdateBalance", () => ({
    useUpdateBalance: () => ({ mutateAsync: mocks.balanceMutate, isPending: false }),
}));

import { AddTransaction } from "./AddTransaction";
import { AccountProvider } from "components";

const account = (controller: string): LogicalAccount =>
    ({ id: "acc-1", currency: "AUD", controller } as LogicalAccount);

const renderModal = (controller = "Manual", onSave = vi.fn()) => {
    const user = userEvent.setup();
    render(
        <AccountProvider account={account(controller)}>
            <AddTransaction show onClose={vi.fn()} onSave={onSave} />
        </AccountProvider>,
    );
    return { user, onSave };
};

const amountInput = () => document.querySelector<HTMLInputElement>("#amount")!;
const newBalanceInput = () => document.querySelector<HTMLInputElement>("#newBalance")!;
const saveButton = () => screen.getByRole("button", { name: "Save" });

beforeEach(() => {
    mocks.createMutate.mockReset().mockResolvedValue(undefined);
    mocks.balanceMutate.mockReset().mockResolvedValue(undefined);
});

describe("AddTransaction", () => {
    describe("Save button enablement", () => {
        it("is disabled when neither amount nor new balance is entered", () => {
            renderModal();
            expect(saveButton()).toBeDisabled();
        });

        it("is enabled once an amount is entered", async () => {
            const { user } = renderModal();
            await user.type(amountInput(), "42");
            expect(saveButton()).toBeEnabled();
        });

        it("is disabled again after the amount is cleared", async () => {
            const { user } = renderModal();
            await user.type(amountInput(), "42");
            await user.clear(amountInput());
            expect(saveButton()).toBeDisabled();
        });
    });

    describe("amount / new-balance mutual exclusivity", () => {
        it("disables the new-balance field once an amount is entered", async () => {
            const { user } = renderModal();
            await user.type(amountInput(), "42");
            expect(newBalanceInput()).toBeDisabled();
            expect(amountInput()).toBeEnabled();
        });

        it("disables the amount field once a new balance is entered", async () => {
            const { user } = renderModal();
            await user.type(newBalanceInput(), "100");
            expect(amountInput()).toBeDisabled();
            expect(newBalanceInput()).toBeEnabled();
        });
    });

    describe("submit routing", () => {
        it("routes an entered amount to create-transaction, not set-balance", async () => {
            const { user, onSave } = renderModal();
            await user.type(amountInput(), "42");
            await user.click(saveButton());

            expect(mocks.createMutate).toHaveBeenCalledTimes(1);
            expect(mocks.createMutate).toHaveBeenCalledWith("acc-1", expect.objectContaining({ amount: 42 }));
            expect(mocks.balanceMutate).not.toHaveBeenCalled();
            expect(onSave).toHaveBeenCalledTimes(1);
        });

        it("routes an entered new balance to set-balance, not create-transaction", async () => {
            const { user } = renderModal();
            await user.type(newBalanceInput(), "1000");
            await user.click(saveButton());

            expect(mocks.balanceMutate).toHaveBeenCalledTimes(1);
            expect(mocks.balanceMutate).toHaveBeenCalledWith("acc-1", expect.objectContaining({ amount: 1000 }));
            expect(mocks.createMutate).not.toHaveBeenCalled();
        });
    });

    describe("blank optional fields", () => {
        it("sends blank description and reference as undefined, not empty strings", async () => {
            const { user } = renderModal();
            await user.type(amountInput(), "42");
            await user.click(saveButton());

            const [, payload] = mocks.createMutate.mock.calls[0];
            expect(payload.description).toBeUndefined();
            expect(payload.reference).toBeUndefined();
            // The date defaults to today (yyyy-MM-dd) and is always sent.
            expect(payload.transactionTime).toMatch(/^\d{4}-\d{2}-\d{2}$/);
        });

        it("passes through a description that was entered", async () => {
            const { user } = renderModal();
            await user.type(amountInput(), "42");
            await user.type(screen.getByRole("textbox", { name: /description/i }), "Coffee");
            await user.click(saveButton());

            const [, payload] = mocks.createMutate.mock.calls[0];
            expect(payload.description).toBe("Coffee");
        });
    });

    describe("non-balance accounts", () => {
        it("shows no new-balance field and routes to create-transaction", async () => {
            const { user } = renderModal("Import");
            expect(newBalanceInput()).toBeNull();

            await user.type(amountInput(), "5");
            await user.click(saveButton());

            expect(mocks.createMutate).toHaveBeenCalledWith("acc-1", expect.objectContaining({ amount: 5 }));
            expect(mocks.balanceMutate).not.toHaveBeenCalled();
        });
    });
});
