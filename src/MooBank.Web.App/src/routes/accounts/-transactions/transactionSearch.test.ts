import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { last12Months, lastMonth } from "utils/dateFns";
import { resolveTransactionSearch } from "./transactionSearch";

const store = (key: string, value: unknown) => localStorage.setItem(key, JSON.stringify(value));
const setUrl = (query: string) => window.history.pushState({}, "", query ? `?${query}` : "?");

beforeEach(() => {
    localStorage.clear();
    setUrl("");
    vi.useFakeTimers();
    // Mid-month so "last month" and "last 12 months" are unambiguous.
    vi.setSystemTime(new Date("2026-07-17T02:00:00Z"));
});

afterEach(() => {
    localStorage.clear();
    setUrl("");
    vi.useRealTimers();
});

describe("resolveTransactionSearch", () => {
    it("defaults an empty search to the last-month period and no filters", () => {
        const result = resolveTransactionSearch({});

        expect(result.tags).toBeUndefined();
        expect(result.tagged).toBeUndefined();
        expect(result.netZero).toBeUndefined();
        expect(result.type).toBeUndefined();
        expect(result.description).toBeUndefined();
        expect(result.start).toBe(lastMonth().startDate.toISOString());
        expect(result.end).toBe(lastMonth().endDate.toISOString());
    });

    it("uses the stored period-id for the date range", () => {
        store("period-id", "5"); // Last 12 months

        const result = resolveTransactionSearch({});

        expect(result.start).toBe(last12Months().startDate.toISOString());
        expect(result.end).toBe(last12Months().endDate.toISOString());
    });

    it("applies persisted localStorage filters when the URL is silent", () => {
        store("filter-tag", [5, 9]);
        store("filter-tagged", true);
        store("filter-netzero", true);
        store("filter-type", "Debit");
        store("filter-description", "coffee");

        const result = resolveTransactionSearch({});

        expect(result.tags).toEqual([5, 9]);
        expect(result.tagged).toBe(true);
        expect(result.netZero).toBe(true);
        expect(result.type).toBe("Debit");
        expect(result.description).toBe("coffee");
    });

    it("lets explicit URL/widget params win over stored filters", () => {
        store("filter-tag", [5]);
        store("filter-type", "Debit");

        const result = resolveTransactionSearch({ tag: "7", type: "Credit" });

        expect(result.tags).toEqual([7]);
        expect(result.type).toBe("Credit");
    });

    it("suppresses stored tagged and description defaults when a widget tag filter is present", () => {
        store("filter-tag", [5]);
        store("filter-tagged", true);
        store("filter-description", "coffee");

        const result = resolveTransactionSearch({ tag: "7" });

        expect(result.tags).toEqual([7]);
        expect(result.tagged).toBeUndefined();
        expect(result.description).toBeUndefined();
    });

    it("keeps an explicit start/end from the URL instead of the default period", () => {
        const result = resolveTransactionSearch({
            start: "2025-01-01T00:00:00.000Z",
            end: "2025-01-31T00:00:00.000Z",
        });

        expect(result.start).toBe("2025-01-01T00:00:00.000Z");
        expect(result.end).toBe("2025-01-31T00:00:00.000Z");
    });

    it("omits an empty stored tag list", () => {
        store("filter-tag", []);

        const result = resolveTransactionSearch({});

        expect(result.tags).toBeUndefined();
    });
});
