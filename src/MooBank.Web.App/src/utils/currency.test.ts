import { describe, it, expect } from "vitest";
import { getCurrencySymbol, formatCurrency } from "utils/currency";

describe("getCurrencySymbol", () => {
    it("returns the known symbol for a currency code", () => {
        expect(getCurrencySymbol("AUD")).toBe("$");
        expect(getCurrencySymbol("GBP")).toBe("£");
        expect(getCurrencySymbol("EUR")).toBe("€");
        expect(getCurrencySymbol("JPY")).toBe("¥");
    });

    it("is case-insensitive", () => {
        expect(getCurrencySymbol("gbp")).toBe("£");
        expect(getCurrencySymbol("eur")).toBe("€");
    });

    it("returns an empty string for null/undefined/empty input", () => {
        expect(getCurrencySymbol(null)).toBe("");
        expect(getCurrencySymbol(undefined)).toBe("");
        expect(getCurrencySymbol("")).toBe("");
    });

    it("falls back to the upper-cased code plus a space for unknown currencies", () => {
        expect(getCurrencySymbol("xyz")).toBe("XYZ ");
        expect(getCurrencySymbol("ZAR")).toBe("ZAR ");
    });

    it("renders CHF with its trailing space", () => {
        expect(getCurrencySymbol("CHF")).toBe("CHF ");
    });
});

describe("formatCurrency", () => {
    it("formats a positive amount with the currency symbol and two decimals", () => {
        expect(formatCurrency(1234.5, "AUD")).toBe("$1,234.50");
    });

    it("prefixes negatives with a minus sign before the symbol", () => {
        expect(formatCurrency(-1234.5, "AUD")).toBe("-$1,234.50");
    });

    it("defaults to AUD when no currency code is given", () => {
        expect(formatCurrency(10)).toBe("$10.00");
    });

    it("treats null/undefined/NaN amounts as zero", () => {
        expect(formatCurrency(null, "AUD")).toBe("$0.00");
        expect(formatCurrency(undefined, "AUD")).toBe("$0.00");
        expect(formatCurrency(Number.NaN, "AUD")).toBe("$0.00");
    });

    it("respects a custom decimal-place count", () => {
        expect(formatCurrency(5, "AUD", 0)).toBe("$5");
        expect(formatCurrency(1.2345, "AUD", 4)).toBe("$1.2345");
    });

    it("uses the fallback symbol for unknown currencies", () => {
        expect(formatCurrency(99, "ZAR")).toBe("ZAR 99.00");
    });
});
