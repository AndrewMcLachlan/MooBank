import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { Transaction, TransactionDeleteImpact } from "api/types.gen";

const mocks = vi.hoisted(() => ({
    impact: vi.fn(),
}));

vi.mock("routes/accounts/-hooks/useTransactionDeleteImpact", () => ({
    useTransactionDeleteImpact: (...args: unknown[]) => mocks.impact(...args),
}));

import { DeleteTransaction } from "./DeleteTransaction";

const transaction = {
    id: "tr-1",
    accountId: "acc-1",
    description: "Bunnings",
    amount: -150,
} as Transaction;

const impactOf = (impact: Partial<TransactionDeleteImpact>) =>
    ({ data: { refunds: [], plannedItems: [], ...impact }, isLoading: false });

const renderDialog = (impact: ReturnType<typeof impactOf>) => {
    mocks.impact.mockReturnValue(impact);
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    const onCancel = vi.fn();
    render(<DeleteTransaction transaction={transaction} show onCancel={onCancel} onConfirm={onConfirm} />);
    return { user, onConfirm, onCancel };
};

beforeEach(() => {
    mocks.impact.mockReset();
});

describe("DeleteTransaction", () => {

    it("names the transaction being deleted", () => {
        renderDialog(impactOf({}));

        expect(screen.getByText("Bunnings")).toBeInTheDocument();
    });

    it("only fetches the impact while the dialog is shown", () => {
        renderDialog(impactOf({}));

        expect(mocks.impact).toHaveBeenCalledWith("acc-1", "tr-1", true);
    });

    it("says nothing about refunds or planned items when there are none", () => {
        renderDialog(impactOf({}));

        expect(screen.queryByText("This transaction is a refund")).not.toBeInTheDocument();
        expect(screen.queryByText("This transaction is a planned payment")).not.toBeInTheDocument();
    });

    it("warns which transactions a refund puts money back on", () => {
        renderDialog(impactOf({ refunds: [{ amount: 50, description: "Bunnings return" }] }));

        expect(screen.getByText("This transaction is a refund")).toBeInTheDocument();
        expect(screen.getByText(/Bunnings return/)).toBeInTheDocument();
    });

    it("warns which planned items the payment will be unlinked from", () => {
        renderDialog(impactOf({ plannedItems: [{ planName: "2027 Plan", itemName: "New fence" }] }));

        expect(screen.getByText("This transaction is a planned payment")).toBeInTheDocument();
        expect(screen.getByText(/New fence/)).toBeInTheDocument();
    });

    it("confirms on Delete and not on Cancel", async () => {
        const { user, onConfirm, onCancel } = renderDialog(impactOf({}));

        await user.click(screen.getByRole("button", { name: "Cancel" }));
        expect(onCancel).toHaveBeenCalledOnce();
        expect(onConfirm).not.toHaveBeenCalled();

        await user.click(screen.getByRole("button", { name: "Delete" }));
        expect(onConfirm).toHaveBeenCalledOnce();
    });
});
