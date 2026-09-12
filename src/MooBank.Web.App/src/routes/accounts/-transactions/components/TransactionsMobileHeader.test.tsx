import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { LogicalAccount } from "api/types.gen";

const mocks = vi.hoisted(() => ({
    stats: { income: 4120, expenses: -2844, net: 1276, total: 41 },
}));

vi.mock("../hooks/useTransactionPeriodStats", () => ({
    useTransactionPeriodStats: () => mocks.stats,
}));

import { TransactionsMobileHeader } from "./TransactionsMobileHeader";
import { AccountProvider } from "components";

const account = {
    id: "acc-1",
    name: "Joint Savings",
    currency: "AUD",
    currentBalance: 12480.22,
} as LogicalAccount;

const renderHeader = () =>
    render(
        <AccountProvider account={account}>
            <TransactionsMobileHeader />
        </AccountProvider>,
    );

describe("TransactionsMobileHeader", () => {

    it("shows the balance under a Balance label", () => {
        renderHeader();
        expect(screen.getByText("Balance")).toBeInTheDocument();
        expect(screen.getByText("$12,480.22")).toBeInTheDocument();
    });

    it("shows the period net, signed", () => {
        renderHeader();
        expect(screen.getByText("Net")).toBeInTheDocument();
        expect(screen.getByText("+$1,276.00")).toBeInTheDocument();
    });

    // The account name is the app bar title and the period is the filter bar's
    // own control; repeating either is what made the old card too tall.
    it("does not repeat the account name, the period or the transaction count", () => {
        const { container } = renderHeader();
        expect(container).not.toHaveTextContent("Joint Savings");
        expect(container).not.toHaveTextContent("41");
    });

    it("shows a negative net signed and in the expense colour", () => {
        mocks.stats = { income: 100, expenses: -512, net: -412, total: 9 };
        renderHeader();
        const net = screen.getByText("-$412.00");
        expect(net).toHaveClass("negative");
    });
});
