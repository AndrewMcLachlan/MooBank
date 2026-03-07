import { useQuery } from "@tanstack/react-query";
import { getAllBillsQueryKey } from "api/@tanstack/react-query.gen";
import { getAllBills } from "api/sdk.gen";
import type { UtilityType, Bill } from "api/types.gen";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import type { BillFilter } from "./types";

export const useAllBills = (pageNumber: number, pageSize: number, filter?: BillFilter) =>
    useQuery({
        queryKey: getAllBillsQueryKey({
            query: {
                PageNumber: pageNumber,
                PageSize: pageSize,
                StartDate: filter?.startDate,
                EndDate: filter?.endDate,
                AccountId: filter?.accountId,
                UtilityType: filter?.utilityType as UtilityType | undefined,
            },
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getAllBills({
                query: {
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    StartDate: filter?.startDate,
                    EndDate: filter?.endDate,
                    AccountId: filter?.accountId,
                    UtilityType: filter?.utilityType as UtilityType | undefined,
                },
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
        },
    });
