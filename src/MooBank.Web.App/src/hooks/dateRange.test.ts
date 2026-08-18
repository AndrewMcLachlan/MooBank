import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { periodOptions } from "models/periodOptions";
import { endOfMonth, startOfMonth } from "date-fns";

import {
    dateRangeLabel,
    dateRangeStorageKey,
    getDateRange,
    isPresetSelection,
    readDateRangeSelection,
    resolveDateRange,
    useDateRange,
    writeDateRangeSelection,
} from "hooks/dateRange";

const setUrlPeriod = (value?: string) => window.history.pushState({}, "", value ? `?period=${value}` : "?");

const store = (value: unknown) => localStorage.setItem(dateRangeStorageKey, JSON.stringify(value));

beforeEach(() => {
    localStorage.clear();
    setUrlPeriod();
});

afterEach(() => {
    localStorage.clear();
    setUrlPeriod();
    vi.useRealTimers();
});

describe("resolveDateRange", () => {
    it("resolves a preset to its periodOptions entry, keeping its live getters", () => {
        expect(resolveDateRange({ preset: "3" })).toBe(periodOptions.find(o => o.value === "3"));
    });

    it("falls back to Last Month when the preset matches no option", () => {
        expect(resolveDateRange({ preset: "not-a-real-option" })).toBe(periodOptions.find(o => o.value === "1"));
    });

    it("resolves a custom range to the whole months it covers", () => {
        const period = resolveDateRange({ startMonth: "2025-03", endMonth: "2025-06" });

        expect(period.startDate).toEqual(startOfMonth(new Date(2025, 2, 1)));
        expect(period.endDate).toEqual(endOfMonth(new Date(2025, 5, 1)));
    });

    it("orders a reversed custom range rather than returning an empty one", () => {
        expect(resolveDateRange({ startMonth: "2025-06", endMonth: "2025-03" }))
            .toEqual(resolveDateRange({ startMonth: "2025-03", endMonth: "2025-06" }));
    });

    it("resolves a single-month range to that month's first and last day", () => {
        const period = resolveDateRange({ startMonth: "2024-02", endMonth: "2024-02" });

        expect(period.startDate).toEqual(new Date(2024, 1, 1));
        expect(period.endDate).toEqual(endOfMonth(new Date(2024, 1, 1)));
    });
});

describe("readDateRangeSelection", () => {
    it("defaults to the Last Month preset when nothing is stored", () => {
        expect(readDateRangeSelection()).toEqual({ preset: "1" });
    });

    it("returns the stored preset", () => {
        store({ preset: "5" });

        expect(readDateRangeSelection()).toEqual({ preset: "5" });
    });

    it("returns a stored custom range", () => {
        store({ startMonth: "2025-03", endMonth: "2025-06" });

        expect(readDateRangeSelection()).toEqual({ startMonth: "2025-03", endMonth: "2025-06" });
    });

    it("prefers a matching ?period= over the stored selection, so widget links can scope the page", () => {
        setUrlPeriod("3");
        store({ preset: "5" });

        expect(readDateRangeSelection()).toEqual({ preset: "3" });
    });

    it("ignores a ?period= that matches no option", () => {
        setUrlPeriod("not-a-real-option");
        store({ preset: "5" });

        expect(readDateRangeSelection()).toEqual({ preset: "5" });
    });

    it("falls back to the default when the stored preset matches no option", () => {
        store({ preset: "not-a-real-option" });

        expect(readDateRangeSelection()).toEqual({ preset: "1" });
    });

    it("falls back to the default when the stored months are not yyyy-MM", () => {
        store({ startMonth: "2025-03-01", endMonth: "June" });

        expect(readDateRangeSelection()).toEqual({ preset: "1" });
    });

    it("falls back to the default when storage holds unparseable text", () => {
        localStorage.setItem(dateRangeStorageKey, "{not json");

        expect(readDateRangeSelection()).toEqual({ preset: "1" });
    });
});

describe("getDateRange", () => {
    it("resolves whatever is stored", () => {
        store({ startMonth: "2025-03", endMonth: "2025-06" });

        expect(getDateRange()).toEqual(resolveDateRange({ startMonth: "2025-03", endMonth: "2025-06" }));
    });

    it("resolves presets live, so a long-lived tab does not serve a stale month", () => {
        vi.useFakeTimers();
        store({ preset: "0" });   // This Month

        vi.setSystemTime(new Date(2026, 4, 15));
        expect(getDateRange().startDate).toEqual(new Date(2026, 4, 1));

        vi.setSystemTime(new Date(2026, 5, 15));
        expect(getDateRange().startDate).toEqual(new Date(2026, 5, 1));
    });
});

describe("dateRangeLabel", () => {
    it("names the preset", () => {
        expect(dateRangeLabel({ preset: "3" })).toBe("Last 3 months");
    });

    it("shows a single month on its own", () => {
        expect(dateRangeLabel({ startMonth: "2025-03", endMonth: "2025-03" })).toBe("Mar 2025");
    });

    it("shows the year once for a range within one year", () => {
        expect(dateRangeLabel({ startMonth: "2025-03", endMonth: "2025-06" })).toBe("Mar – Jun 2025");
    });

    it("shows both years for a range that spans them", () => {
        expect(dateRangeLabel({ startMonth: "2024-11", endMonth: "2025-06" })).toBe("Nov 2024 – Jun 2025");
    });
});

describe("isPresetSelection", () => {
    it("distinguishes the two shapes", () => {
        expect(isPresetSelection({ preset: "1" })).toBe(true);
        expect(isPresetSelection({ startMonth: "2025-03", endMonth: "2025-06" })).toBe(false);
    });
});

describe("useDateRange", () => {
    it("starts from the stored selection", () => {
        store({ preset: "5" });

        const { result } = renderHook(() => useDateRange());

        expect(result.current.selection).toEqual({ preset: "5" });
        expect(result.current.period).toBe(periodOptions.find(o => o.value === "5"));
    });

    it("persists a new selection so it is still there on the next page", () => {
        const { result } = renderHook(() => useDateRange());

        act(() => result.current.setSelection({ startMonth: "2025-03", endMonth: "2025-06" }));

        expect(result.current.selection).toEqual({ startMonth: "2025-03", endMonth: "2025-06" });
        expect(readDateRangeSelection()).toEqual({ startMonth: "2025-03", endMonth: "2025-06" });
    });

    it("round-trips a written selection", () => {
        writeDateRangeSelection({ startMonth: "2023-01", endMonth: "2023-12" });

        expect(readDateRangeSelection()).toEqual({ startMonth: "2023-01", endMonth: "2023-12" });
    });
});
