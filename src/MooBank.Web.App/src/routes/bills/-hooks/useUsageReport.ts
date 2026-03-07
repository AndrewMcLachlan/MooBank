import { useQuery } from "@tanstack/react-query";
import { getUsageReportOptions } from "api/@tanstack/react-query.gen";
import type { UtilityType } from "api/types.gen";

export const useUsageReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getUsageReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});
