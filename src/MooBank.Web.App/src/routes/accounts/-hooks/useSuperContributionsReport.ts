import { useQuery } from "@tanstack/react-query";
import { superContributionsReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useSuperContributionsReport = (accountId: string, start: Date, end: Date, enabled: boolean) =>
    useQuery({
        ...superContributionsReportOptions({
            path: {
                accountId,
                start: formatISODate(start),
                end: formatISODate(end),
            },
        }),
        enabled: enabled && !!accountId && !!start && !!end,
    });
