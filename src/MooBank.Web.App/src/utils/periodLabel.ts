import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";

import { periodOptions } from "models/periodOptions";

export interface PeriodFilter {
    start?: string;
    end?: string;
}

export const getPeriodLabel = (filter: PeriodFilter | undefined): string => {
    if (!filter?.start || !filter?.end) return "";

    const matched = periodOptions.find(o =>
        o.startDate?.toISOString() === filter.start &&
        o.endDate?.toISOString() === filter.end,
    );

    if (matched) return matched.label;

    return `${format(parseISO(filter.start), "dd MMM yyyy")} → ${format(parseISO(filter.end), "dd MMM yyyy")}`;
};
