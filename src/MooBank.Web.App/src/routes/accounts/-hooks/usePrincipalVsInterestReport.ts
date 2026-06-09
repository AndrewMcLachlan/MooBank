import { useQuery } from "@tanstack/react-query";
import { principalVsInterestReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const usePrincipalVsInterestReport = (accountId: string, start: Date, end: Date, enabled: boolean) =>
    useQuery({
        ...principalVsInterestReportOptions({
            path: {
                accountId,
                start: formatISODate(start),
                end: formatISODate(end),
            },
        }),
        enabled: enabled && !!accountId && !!start && !!end,
    });
