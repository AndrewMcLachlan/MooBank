import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import type { Account } from "api/types.gen";
import { billPeriodOptions } from "./billPeriodOptions";

const today = new Date(2026, 7, 31);

beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(today);
});

afterEach(() => vi.useRealTimers());

const account = (id: string, latestBill: string | null, billingIntervalDays: number | null): Account =>
    ({ id, latestBill, billingIntervalDays } as unknown as Account);

const labels = (accounts: Account[] | undefined, accountId?: string) =>
    billPeriodOptions(accounts, accountId).map(o => o.label);

const monthly = account("m", "2026-08-25", 30);
const quarterly = account("q", "2026-08-25", 91);

describe("billPeriodOptions", () => {
    /**
     * Given an account billed monthly
     * When the periods are built
     * Then every window is offered, shortest first.
     */
    it("offers the full list for a monthly account", () => {
        expect(labels([monthly])).toEqual([
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
    /* On a quarterly bill those are one bill and two -- which the first two entries already say,
       and say exactly. */
    it("drops the short windows for a quarterly account", () => {
        expect(labels([quarterly])).toEqual([
            "Last period",
            "Previous Period",
            "Last 12 Months",
            "This Year",
            "Last Year",
            "All Time",
        ]);
    });

    /**
     * Given accounts on different cadences
     * When no one account is selected
     * Then the shorter windows stay, because for some of them they still mean something.
     */
    it("keeps the short windows for a mixed set", () => {
        expect(labels([monthly, quarterly])).toContain("Last 3 Months");
    });

    /**
     * Given a quarterly account is selected out of a mixed set
     * When the periods are built
     * Then the list suits that account.
     */
    it("follows the selected account", () => {
        expect(labels([monthly, quarterly], "q")).not.toContain("Last 3 Months");
        expect(labels([monthly, quarterly], "m")).toContain("Last 3 Months");
    });

    /**
     * Given the named periods
     * When they are built
     * Then they carry no dates: the server answers them from the bills.
     */
    it("leaves the named periods without dates", () => {
        const options = billPeriodOptions([monthly]);

        expect(options.find(o => o.value === "Last")?.period).toBeUndefined();
        expect(options.find(o => o.value === "Previous")?.period).toBeUndefined();
        expect(options.find(o => o.value === "12")?.period).toBeDefined();
    });

    /**
     * Given an account with a single bill
     * When the periods are built
     * Then there is a last period but no previous one.
     */
    /* An account is only known to have two bills once there is an interval between them. */
    it("offers no previous period until there are two bills", () => {
        const oneBill = account("one", "2026-08-25", null);

        expect(labels([oneBill])).toContain("Last period");
        expect(labels([oneBill])).not.toContain("Previous Period");
    });

    /**
     * Given an account that has never been billed
     * When the periods are built
     * Then only the calendar windows are offered.
     */
    it("offers only the calendar windows for an account with no bills", () => {
        expect(labels([account("new", null, null)])).toEqual([
            "Last 3 Months",
            "Last 6 Months",
            "Last 12 Months",
            "This Year",
            "Last Year",
            "All Time",
        ]);

        expect(labels(undefined)).toEqual(labels([]));
    });

    /* The dates are getters, as the app-wide options are, so a tab left open overnight does not
       keep serving yesterday's idea of "This Year". */
    it("reads the calendar windows when they are used, not when built", () => {
        const thisYear = billPeriodOptions([monthly]).find(o => o.label === "This Year")!;

        vi.setSystemTime(new Date(2027, 0, 5));

        expect(thisYear.period!.startDate.getFullYear()).toBe(2027);
    });
});
