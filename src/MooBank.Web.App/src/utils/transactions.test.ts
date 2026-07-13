import { describe, it, expect } from "vitest";
import type { TransactionType, TransactionSplit } from "api/types.gen";
import { isCredit, isDebit, getSplitTotal } from "utils/transactions";

// The generated TransactionType union currently only includes "NotSet" | "Credit" | "Debit"
// (see MooBank.Primitives/TransactionType.cs), but the implementation's internal enum also
// defines RecurringCredit/RecurringDebit/BalanceAdjustment. Those values aren't reachable via
// the real API today, so they're cast through `unknown` here to exercise the extra branches.
const recurringCredit = "RecurringCredit" as unknown as TransactionType;
const recurringDebit = "RecurringDebit" as unknown as TransactionType;
const balanceAdjustment = "BalanceAdjustment" as unknown as TransactionType;

describe("isCredit", () => {
    it("is true for Credit", () => {
        expect(isCredit("Credit")).toBe(true);
    });

    it("is false for Debit", () => {
        expect(isCredit("Debit")).toBe(false);
    });

    it("is true for RecurringCredit (odd enum value)", () => {
        expect(isCredit(recurringCredit)).toBe(true);
    });

    it("is false for RecurringDebit (even enum value)", () => {
        expect(isCredit(recurringDebit)).toBe(false);
    });

    it("is true for BalanceAdjustment (odd enum value)", () => {
        expect(isCredit(balanceAdjustment)).toBe(true);
    });
});

describe("isDebit", () => {
    it("is false for Credit", () => {
        expect(isDebit("Credit")).toBe(false);
    });

    it("is true for Debit", () => {
        expect(isDebit("Debit")).toBe(true);
    });

    it("is false for RecurringCredit (odd enum value)", () => {
        expect(isDebit(recurringCredit)).toBe(false);
    });

    it("is true for RecurringDebit (even enum value)", () => {
        expect(isDebit(recurringDebit)).toBe(true);
    });

    it("is false for BalanceAdjustment (odd enum value)", () => {
        expect(isDebit(balanceAdjustment)).toBe(false);
    });
});

const makeSplit = (amount: number | string): TransactionSplit => ({
    id: "1",
    tags: [],
    amount: amount as number,
    offsetBy: [],
});

describe("getSplitTotal", () => {
    it("returns 0 for an empty array", () => {
        expect(getSplitTotal([])).toBe(0);
    });

    it("sums numeric amounts", () => {
        const splits = [makeSplit(10), makeSplit(20.5), makeSplit(-5)];
        expect(getSplitTotal(splits)).toBe(25.5);
    });

    it("sums string amounts by coercing them to numbers", () => {
        const splits = [makeSplit("10"), makeSplit("15.25"), makeSplit(4.75)];
        expect(getSplitTotal(splits)).toBe(30);
    });
});
