import { useQuery } from "@tanstack/react-query";
import { getBillsForAnAccountQueryKey } from "api/@tanstack/react-query.gen";
import { getBillsForAnAccount } from "api/sdk.gen";
import type { Bill } from "api/types.gen";
import { PagedResult } from "@andrewmclachlan/moo-ds";

export const useBills = (id: string, pageNumber: number, pageSize: number) => useQuery({
    queryKey: getBillsForAnAccountQueryKey({
        path: { instrumentId: id },
        query: { PageNumber: pageNumber, PageSize: pageSize },
    }),
    queryFn: async ({ signal }) => {
        const { data, headers } = await getBillsForAnAccount({
            path: { instrumentId: id },
            query: { PageNumber: pageNumber, PageSize: pageSize },
            signal,
            throwOnError: true,
        });
        return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
    },
});
