import { useQuery } from "@tanstack/react-query";
import { getCostPerUnitReportOptions } from "api/@tanstack/react-query.gen";
import type { UtilityType } from "api/types.gen";

export const useCostPerUnitReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getCostPerUnitReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});
