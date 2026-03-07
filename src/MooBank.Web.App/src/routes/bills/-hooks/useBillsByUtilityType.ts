import { useQuery } from "@tanstack/react-query";
import { getBillsByUtilityTypeQueryKey } from "api/@tanstack/react-query.gen";
import { getBillsByUtilityType } from "api/sdk.gen";
import type { UtilityType, Bill } from "api/types.gen";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import type { BillFilter } from "./types";

export const useBillsByUtilityType = (utilityType: string, pageNumber: number, pageSize: number, filter?: BillFilter) => useQuery({
    queryKey: getBillsByUtilityTypeQueryKey({
        path: { utilityType: utilityType as UtilityType },
        query: {
            PageNumber: pageNumber,
            PageSize: pageSize,
            StartDate: filter?.startDate,
            EndDate: filter?.endDate,
            AccountId: filter?.accountId,
        },
    }),
    queryFn: async ({ signal }) => {
        const { data, headers } = await getBillsByUtilityType({
            path: { utilityType: utilityType as UtilityType },
            query: {
                PageNumber: pageNumber,
                PageSize: pageSize,
                StartDate: filter?.startDate,
                EndDate: filter?.endDate,
                AccountId: filter?.accountId,
            },
            signal,
            throwOnError: true,
        });
        return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
    },
    enabled: !!utilityType,
});
