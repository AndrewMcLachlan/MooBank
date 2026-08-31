import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import type { Bill } from "api/types.gen";
import { formatISODate } from "utils/dateFns";
import { billPeriodOptions } from "./billPeriodOptions";

const today = new Date(2026, 7, 31);

beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(today);
});

afterEach(() => vi.useRealTimers());

/** A bill covering one period, dated by its end. */
const bill = (start: string, end: string): Bill => ({
    periods: [{ periodStart: start, periodEnd: end }],
} as unknown as Bill);

const labels = (bills: Bill[] | undefined) => billPeriodOptions(bills).map(o => o.label);

const monthly = [
    bill("2026-07-26", "2026-08-25"),
    bill("2026-06-26", "2026-07-25"),
    bill("2026-05-26", "2026-06-25"),
];

const quarterly = [
    bill("2026-05-26", "2026-08-25"),
    bill("2026-02-26", "2026-05-25"),
];

describe("billPeriodOptions", () => {
    /**
     * Given an account billed monthly
     * When the periods are built
     * Then every window is offered, shortest first.
     */
    it("offers the full list for a monthly account", () => {
        expect(labels(monthly)).toEqual([
            "Last period",
            "Previous Period",
            "Last 3 Months",
            "Last 6 Months",
            "Last 12 Months",
            "This Year",
            "Last Year",
            "All Time",
        ]);
    });

    /**
     * Given an account billed quarterly
     * When the periods are built
     * Then the three and six month windows are left out.
     */
    /* On a quarterly bill those are one bill and two -- which "Last period" and "Previous Period"
       already say, and say exactly. */
    it("drops the short windows for a quarterly account", () => {
        expect(labels(quarterly)).toEqual([
            "Last period",
            "Previous Period",
            "Last 12 Months",
            "This Year",
            "Last Year",
            "All Time",
        ]);
    });

    /**
     * Given the bills on hand
     * When "Last period" is resolved
     * Then it is the most recent period actually billed, not an approximation of it.
     */
    it("resolves the last period to the most recent bill", () => {
        const [last] = billPeriodOptions(monthly);

        expect(formatISODate(last.startDate)).toBe("2026-07-26");
        expect(formatISODate(last.endDate)).toBe("2026-08-25");
    });

    it("resolves the previous period to the bill before it", () => {
        const previous = billPeriodOptions(monthly)[1];

        expect(formatISODate(previous.startDate)).toBe("2026-06-26");
        expect(formatISODate(previous.endDate)).toBe("2026-07-25");
    });

    /**
     * Given bills in no particular order
     * When the periods are built
     * Then the most recent is still first.
     */
    it("orders by period end, whatever order the bills arrive in", () => {
        const [last] = billPeriodOptions([monthly[2], monthly[0], monthly[1]]);

        expect(formatISODate(last.endDate)).toBe("2026-08-25");
    });

    /**
     * Given a bill covering several periods
     * When its span is taken
     * Then it runs from the earliest start to the latest end.
     */
    it("spans a bill with more than one period", () => {
        const split = { periods: [
            { periodStart: "2026-07-01", periodEnd: "2026-07-15" },
            { periodStart: "2026-07-16", periodEnd: "2026-07-31" },
        ] } as unknown as Bill;

        const [last] = billPeriodOptions([split]);

        expect(formatISODate(last.startDate)).toBe("2026-07-01");
        expect(formatISODate(last.endDate)).toBe("2026-07-31");
    });

    /**
     * Given no bills, because the filter excluded them all or none exist yet
     * When the periods are built
     * Then only the calendar windows are offered.
     */
    it("offers only the calendar windows with no bills", () => {
        expect(labels([])).toEqual([
            "Last 3 Months",
            "Last 6 Months",
            "Last 12 Months",
            "This Year",
            "Last Year",
            "All Time",
        ]);

        expect(labels(undefined)).toEqual(labels([]));
    });

    /**
     * Given a single bill
     * When the periods are built
     * Then there is a last period but no previous one.
     */
    it("offers no previous period when there is only one bill", () => {
        expect(labels([monthly[0]])).not.toContain("Previous Period");
        expect(labels([monthly[0]])).toContain("Last period");
    });

    /* The dates are getters, as the app-wide options are, so a tab left open overnight does not
       keep serving yesterday's idea of "This Year". */
    it("reads the calendar windows when they are used, not when built", () => {
        const options = billPeriodOptions(monthly);
        const thisYear = options.find(o => o.label === "This Year")!;

        vi.setSystemTime(new Date(2027, 0, 5));

        expect(thisYear.startDate.getFullYear()).toBe(2027);
    });
});
