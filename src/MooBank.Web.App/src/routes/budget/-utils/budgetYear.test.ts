import { describe, it, expect } from "vitest";
import { isBudgetYear } from "./budgetYear";

describe("isBudgetYear", () => {
    it("accepts a plausible four-digit year", () => {
        expect(isBudgetYear("2024")).toBe(true);
        expect(isBudgetYear(2024)).toBe(true);
    });

    it("rejects anything that would reach the API as NaN", () => {
        // The year comes straight from the URL, so these are all reachable by hand-typing.
        expect(isBudgetYear("abc")).toBe(false);
        expect(isBudgetYear("")).toBe(false);
        expect(isBudgetYear(undefined)).toBe(false);
        expect(isBudgetYear(null)).toBe(false);
    });

    it("rejects non-integers, which would not round-trip through the route", () => {
        expect(isBudgetYear("2024.5")).toBe(false);
    });

    it("rejects years outside a plausible range", () => {
        expect(isBudgetYear("1066")).toBe(false);
        expect(isBudgetYear("99999")).toBe(false);
        expect(isBudgetYear("-2024")).toBe(false);
    });
});
