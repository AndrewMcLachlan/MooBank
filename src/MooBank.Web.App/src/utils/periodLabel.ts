import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";

import { periodOptions } from "models/periodOptions";
import { formatISODate } from "utils/dateFns";

export interface PeriodFilter {
    start?: string;
    end?: string;
}

/**
 * Names the period a transaction filter covers: the preset's own label where the range is one of
 * the ready-made periods, otherwise the dates themselves. The filter carries instants (the API
 * filters on TransactionTime), so presets are matched on the calendar days those instants fall on
 * rather than to the millisecond.
 */
export const getPeriodLabel = (filter: PeriodFilter | undefined): string => {
    if (!filter?.start || !filter?.end) return "";

    const start = parseISO(filter.start);
    const end = parseISO(filter.end);

    const matched = periodOptions.find(o =>
        o.startDate && formatISODate(o.startDate) === formatISODate(start) &&
        o.endDate && formatISODate(o.endDate) === formatISODate(end),
    );

    if (matched) return matched.label;

    return `${format(start, "dd MMM yyyy")} → ${format(end, "dd MMM yyyy")}`;
};
