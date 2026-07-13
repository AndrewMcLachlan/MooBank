import { describe, it, expect } from "vitest";
import type { TransactionSplit } from "api/types.gen";
import { isCredit, isDebit, getSplitTotal } from "utils/transactions";

// TransactionType is NotSet | Credit | Debit (see MooBank.Primitives/TransactionType.cs).

describe("isCredit", () => {
    it("is true for Credit", () => {
        expect(isCredit("Credit")).toBe(true);
    });

    it("is false for Debit", () => {
        expect(isCredit("Debit")).toBe(false);
    });

    it("is false for NotSet", () => {
        expect(isCredit("NotSet")).toBe(false);
    });
});

describe("isDebit", () => {
    it("is false for Credit", () => {
        expect(isDebit("Credit")).toBe(false);
    });

    it("is true for Debit", () => {
        expect(isDebit("Debit")).toBe(true);
    });

    it("is false for NotSet", () => {
        expect(isDebit("NotSet")).toBe(false);
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
