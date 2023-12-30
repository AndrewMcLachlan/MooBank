import { parseISO } from "date-fns/parseISO";
import { Period, lastMonth } from "./dateFns";

export const getCachedPeriod = (period?: Period) => {
    const periodCache = window.localStorage.getItem("report-period");
    if (periodCache === null) return period ?? lastMonth;

    const result = JSON.parse(periodCache);

    result.startDate = parseISO(result.startDate);
    result.endDate = parseISO(result.endDate);

    return result as Period;
}