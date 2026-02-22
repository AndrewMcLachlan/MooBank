import { last12Months, last3Months, last6Months, lastMonth, lastYear, thisMonth, thisYear, allTime, previousMonth } from "utils/dateFns";
import type { Period } from "models/dateFns";

export interface PeriodOption extends Period {
    value: string,
    label: string,
}

export const periodOptions: PeriodOption[] = [
    { value: "0", label: "This Month", ...thisMonth },
    { value: "1", label: "Last Month", ...lastMonth },
    { value: "2", label: "Previous Month", ...previousMonth },
    { value: "3", label: "Last 3 months", ...last3Months },
    { value: "4", label: "Last 6 months", ...last6Months },
    { value: "5", label: "Last 12 months", ...last12Months },
    { value: "8", label: "This Year", ...thisYear },
    { value: "6", label: "Last year", ...lastYear },
    { value: "7", label: "All time", ...allTime },
];
