import type { Account } from "api/types.gen";
import type { DayRangePreset } from "components/DayRangeSelector";
import { allTime, last12Months, last3Months, last6Months, lastYear, thisYear } from "utils/dateFns";

/**
 * A billing interval long enough that the shorter calendar windows stop saying anything. At 70 days
 * a quarterly account is caught while a monthly one, even a long month, is not.
 */
const quarterlyDays = 70;

const calendar = (value: string, label: string, period: () => { startDate: Date; endDate: Date }): DayRangePreset => ({
    value,
    label,
    // A getter, as the app-wide options are: read when used, not when the list is built, so a tab
    // left open overnight does not keep serving yesterday's "This Year".
    get period() { return period(); },
});

/**
 * The ready-made periods offered by the bills filter.
 *
 * "Last period" and "Previous Period" carry no dates. Which days those cover is something only the
 * bills know, so they go to the server by name and it answers from the bills themselves -- rather
 * than the client fetching bills to work out what to ask for.
 *
 * The rest are calendar ranges, except that a quarterly account leaves out the three and six month
 * windows: on a quarterly bill those are one bill and two, which the first two entries already say,
 * and say exactly. Cadence comes from the accounts, which carry it.
 */
export const billPeriodOptions = (accounts: Account[] | undefined, accountId?: string): DayRangePreset[] => {

    const inScope = (accounts ?? []).filter(a => !accountId || a.id === accountId);

    const options: DayRangePreset[] = [];

    if (inScope.some(a => a.latestBill)) options.push({ value: "Last", label: "Last period" });

    // An interval needs two bills to measure between, which is also what a previous period needs.
    const intervals = inScope.map(a => a.billingIntervalDays).filter((d): d is number => typeof d === "number");

    if (intervals.length > 0) options.push({ value: "Previous", label: "Previous Period" });

    // Only when every account in scope is billed that way: a mixed set keeps the shorter windows,
    // because for some of those accounts they still mean something.
    const quarterly = intervals.length > 0 && intervals.every(d => d >= quarterlyDays);

    if (!quarterly) {
        options.push(calendar("3", "Last 3 Months", last3Months));
        options.push(calendar("6", "Last 6 Months", last6Months));
    }

    options.push(calendar("12", "Last 12 Months", last12Months));
    options.push(calendar("this-year", "This Year", thisYear));
    options.push(calendar("last-year", "Last Year", lastYear));
    options.push(calendar("all", "All Time", allTime));

    return options;
};
