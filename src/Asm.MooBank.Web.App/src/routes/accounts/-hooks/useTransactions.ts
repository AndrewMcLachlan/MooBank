import { useQuery } from "@tanstack/react-query";
import { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import type { Transaction, TransactionFilterType, SortDirection as GenSortDirection } from "api/types.gen";
import { TransactionsFilter } from "store/state";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
} from "api/@tanstack/react-query.gen";
import { getTransactions, getUntaggedTransactions } from "api/sdk.gen";

export const useTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    const queryParams = {
        Filter: filter.description || undefined,
        Start: filter.start || undefined,
        End: filter.end || undefined,
        TagIds: filter.tags,
        SortField: sortField || undefined,
        TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
        SortDirection: (sortDirection || "Descending") as GenSortDirection,
        ExcludeNetZero: filter.filterNetZero || undefined,
    };

    const tagged = filter.filterTagged;

    const untaggedResult = useQuery({
        queryKey: getUntaggedTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
            query: queryParams,
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getUntaggedTransactions({
                path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
                query: queryParams,
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
        },
        enabled: !!accountId && !!filter?.start && !!filter?.end && tagged,
    });

    const regularResult = useQuery({
        queryKey: getTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: queryParams,
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getTransactions({
                path: { instrumentId: accountId, pageSize, pageNumber },
                query: queryParams,
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
        },
        enabled: !!accountId && !!filter?.start && !!filter?.end && !tagged,
    });

    return tagged ? untaggedResult : regularResult;
}
