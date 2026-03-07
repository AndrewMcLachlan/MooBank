import { useQuery } from "@tanstack/react-query";
import { getServiceChargeReportOptions } from "api/@tanstack/react-query.gen";
import type { UtilityType } from "api/types.gen";

export const useServiceChargeReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getServiceChargeReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});
