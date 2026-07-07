import { last12Months, last3Months, last6Months, lastMonth, lastYear, thisMonth, thisYear, allTime, previousMonth } from "utils/dateFns";
import type { Period } from "models/dateFns";

export interface PeriodOption extends Period {
    value: string,
    label: string,
}

// startDate/endDate are getters so the dates are evaluated when accessed,
// not once at module load (which goes stale in long-lived tabs).
const option = (value: string, label: string, period: () => Period): PeriodOption => ({
    value,
    label,
    get startDate() { return period().startDate; },
    get endDate() { return period().endDate; },
});

export const periodOptions: PeriodOption[] = [
    option("0", "This Month", thisMonth),
    option("1", "Last Month", lastMonth),
    option("2", "Previous Month", previousMonth),
    option("3", "Last 3 months", last3Months),
    option("4", "Last 6 months", last6Months),
    option("5", "Last 12 months", last12Months),
    option("8", "This Year", thisYear),
    option("6", "Last year", lastYear),
    option("7", "All time", allTime),
];
