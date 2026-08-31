import { parseISO } from "date-fns";

import type { Bill } from "api/types.gen";
import type { Period } from "models/dateFns";
import type { PeriodOption } from "models/periodOptions";
import { allTime, last12Months, last3Months, last6Months, lastYear, thisYear } from "utils/dateFns";

/**
 * A bill period long enough that the shorter calendar windows stop saying anything. At 70 days a
 * quarterly bill is caught while a monthly one, even a long one, is not.
 */
const quarterlyDays = 70;

const option = (value: string, label: string, period: () => Period): PeriodOption => ({
    value,
    label,
    // Getters, as the app-wide options are: the dates are read when used, not when the list is
    // built, so a tab left open overnight does not keep serving yesterday's "This Year".
    get startDate() { return period().startDate; },
    get endDate() { return period().endDate; },
});

/** The span a bill covers: its earliest period start to its latest period end. */
const billPeriod = (bill: Bill): Period | undefined => {
    const periods = bill.periods ?? [];
    if (periods.length === 0) return undefined;

    return {
        startDate: parseISO(periods.reduce((earliest, p) => p.periodStart < earliest ? p.periodStart : earliest, periods[0].periodStart)),
        endDate: parseISO(periods.reduce((latest, p) => p.periodEnd > latest ? p.periodEnd : latest, periods[0].periodEnd)),
    };
};

const daysIn = (period: Period) =>
    Math.round((period.endDate.getTime() - period.startDate.getTime()) / 86_400_000) + 1;

/**
 * The ready-made periods offered by the bills filter.
 *
 * Two of them are the bills' own: "Last period" and "Previous Period" are the periods actually
 * billed, not a calendar window that approximates them. The rest are calendar ranges, except that a
 * quarterly account drops the three and six month entries -- on a quarterly bill those are one bill
 * and two, which is what "Last period" and "Previous Period" already say better.
 *
 * Built from whatever bills are on hand. With none, only the calendar entries are offered.
 */
export const billPeriodOptions = (bills: Bill[] | undefined): PeriodOption[] => {

    const periods = (bills ?? [])
        .map(billPeriod)
        .filter((p): p is Period => p !== undefined)
        .sort((a, b) => b.endDate.getTime() - a.endDate.getTime());

    const options: PeriodOption[] = [];

    if (periods[0]) options.push(option("last-period", "Last period", () => periods[0]));
    if (periods[1]) options.push(option("previous-period", "Previous Period", () => periods[1]));

    // Judged on the most recent period: an account that has changed cadence is billed the way it is
    // billed now, not the way it used to be.
    const quarterly = periods[0] !== undefined && daysIn(periods[0]) >= quarterlyDays;

    if (!quarterly) {
        options.push(option("3", "Last 3 Months", last3Months));
        options.push(option("6", "Last 6 Months", last6Months));
    }

    options.push(option("12", "Last 12 Months", last12Months));
    options.push(option("this-year", "This Year", thisYear));
    options.push(option("last-year", "Last Year", lastYear));
    options.push(option("all", "All Time", allTime));

    return options;
};
