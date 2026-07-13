import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { renderHook } from "@testing-library/react";
import { parseISO } from "date-fns/parseISO";
import { periodOptions } from "models/periodOptions";
import { lastMonth } from "utils/dateFns";

// Stable spy shared with the mocked module (hoisted above vi.mock).
const mocks = vi.hoisted(() => ({
    useLocalStorage: vi.fn(),
}));

vi.mock("@andrewmclachlan/moo-ds", () => ({
    useLocalStorage: mocks.useLocalStorage,
}));

import { getPeriod, useCustomPeriod } from "hooks/period";

const setUrlPeriod = (value: string | undefined) => {
    window.history.pushState({}, "", value ? `?period=${value}` : "?");
};

beforeEach(() => {
    localStorage.clear();
    window.history.pushState({}, "", "?");
});

afterEach(() => {
    localStorage.clear();
    window.history.pushState({}, "", "?");
    mocks.useLocalStorage.mockReset();
    vi.restoreAllMocks();
});

describe("getPeriod", () => {
    it("prefers a matching period from the URL query string over local storage", () => {
        setUrlPeriod("3");
        localStorage.setItem("period-id", JSON.stringify("5"));

        const result = getPeriod();

        expect(result).toBe(periodOptions.find(o => o.value === "3"));
    });

    it("falls back to period-id when the URL period does not match any option", () => {
        setUrlPeriod("not-a-real-option");
        localStorage.setItem("period-id", JSON.stringify("3"));

        const result = getPeriod();

        expect(result).toBe(periodOptions.find(o => o.value === "3"));
    });

    it("uses the stored period-id when there is no URL period", () => {
        localStorage.setItem("period-id", JSON.stringify("3"));

        const result = getPeriod();

        expect(result).toBe(periodOptions.find(o => o.value === "3"));
    });

    it("defaults period-id to \"1\" (Last Month) when nothing is stored", () => {
        const result = getPeriod();

        expect(result).toBe(periodOptions.find(o => o.value === "1"));
    });

    it("falls back to the first period option when the stored period-id matches nothing", () => {
        localStorage.setItem("period-id", JSON.stringify("not-a-real-option"));

        const result = getPeriod();

        expect(result).toBe(periodOptions[0]);
    });

    it("reads a stored custom period and hydrates ISO date strings to Date objects when period-id is -1", () => {
        localStorage.setItem("period-id", JSON.stringify("-1"));
        localStorage.setItem("period", JSON.stringify({
            startDate: "2026-01-01T00:00:00.000Z",
            endDate: "2026-01-31T00:00:00.000Z",
        }));

        const result = getPeriod();

        expect(result.startDate).toBeInstanceOf(Date);
        expect(result.endDate).toBeInstanceOf(Date);
        expect(result.startDate).toEqual(parseISO("2026-01-01T00:00:00.000Z"));
        expect(result.endDate).toEqual(parseISO("2026-01-31T00:00:00.000Z"));
    });

    it("falls back to last month when period-id is -1 but no custom period is stored", () => {
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2026-06-15T12:00:00Z"));

        localStorage.setItem("period-id", JSON.stringify("-1"));

        const result = getPeriod();

        expect(result).toEqual(lastMonth());

        vi.useRealTimers();
    });
});

describe("useCustomPeriod", () => {
    it("initialises useLocalStorage with the \"period\" key and a last-month default", () => {
        mocks.useLocalStorage.mockReturnValue([{ startDate: new Date(), endDate: new Date() }, vi.fn()]);

        renderHook(() => useCustomPeriod());

        expect(mocks.useLocalStorage).toHaveBeenCalledWith("period", expect.objectContaining({
            startDate: expect.any(Date),
            endDate: expect.any(Date),
        }));
    });

    it("hydrates ISO string dates from storage into Date objects", () => {
        mocks.useLocalStorage.mockReturnValue([
            { startDate: "2026-02-01T00:00:00.000Z", endDate: "2026-02-28T00:00:00.000Z" },
            vi.fn(),
        ]);

        const { result } = renderHook(() => useCustomPeriod());
        const [period] = result.current;

        expect(period.startDate).toBeInstanceOf(Date);
        expect(period.endDate).toBeInstanceOf(Date);
        expect(period.startDate.toISOString()).toBe("2026-02-01T00:00:00.000Z");
        expect(period.endDate.toISOString()).toBe("2026-02-28T00:00:00.000Z");
    });

    it("leaves already-hydrated Date values untouched", () => {
        const startDate = new Date("2026-03-01T00:00:00.000Z");
        const endDate = new Date("2026-03-31T00:00:00.000Z");
        mocks.useLocalStorage.mockReturnValue([{ startDate, endDate }, vi.fn()]);

        const { result } = renderHook(() => useCustomPeriod());
        const [period] = result.current;

        expect(period.startDate).toBe(startDate);
        expect(period.endDate).toBe(endDate);
    });

    it("returns the setter from useLocalStorage unchanged", () => {
        const setPeriod = vi.fn();
        mocks.useLocalStorage.mockReturnValue([{ startDate: new Date(), endDate: new Date() }, setPeriod]);

        const { result } = renderHook(() => useCustomPeriod());
        const [, setter] = result.current;

        expect(setter).toBe(setPeriod);
    });
});
