import { useQuery } from "@tanstack/react-query";
import { superReturnsReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useSuperReturnsReport = (accountId: string, start: Date, end: Date, enabled: boolean) =>
    useQuery({
        ...superReturnsReportOptions({
            path: {
                accountId,
                start: formatISODate(start),
                end: formatISODate(end),
            },
        }),
        enabled: enabled && !!accountId && !!start && !!end,
    });
