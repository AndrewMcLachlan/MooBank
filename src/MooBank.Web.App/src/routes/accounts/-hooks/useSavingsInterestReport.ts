import { useQuery } from "@tanstack/react-query";
import { savingsInterestReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useSavingsInterestReport = (accountId: string, start: Date, end: Date, enabled: boolean) =>
    useQuery({
        ...savingsInterestReportOptions({
            path: {
                accountId,
                start: formatISODate(start),
                end: formatISODate(end),
            },
        }),
        enabled: enabled && !!accountId && !!start && !!end,
    });
