import { useQuery } from "@tanstack/react-query";
import type { QueryClient } from "@tanstack/react-query";
import type { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import type { Transaction, TransactionFilterType, SortDirection as GenSortDirection } from "api/types.gen";
import type { TransactionsFilter } from "models/transactions";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
} from "api/@tanstack/react-query.gen";
import { getTransactions, getUntaggedTransactions } from "api/sdk.gen";

// Single source of the query key + fetch for the transaction list, so the component hook and the
// route loader (which warms the cache) always agree on the key. `tagged` selects the untagged
// endpoint. `enabled` mirrors the guard that a date range must be present.
export const transactionsQueryConfig = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    const path = { instrumentId: accountId, pageSize, pageNumber };

    const query = {
        Filter: filter.description || undefined,
        Start: filter.start || undefined,
        End: filter.end || undefined,
        TagIds: filter.tags,
        SortField: sortField || undefined,
        TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
        SortDirection: (sortDirection || "Descending") as GenSortDirection,
        ExcludeNetZero: filter.filterNetZero || undefined,
    };

    const enabled = !!accountId && !!filter?.start && !!filter?.end;

    if (filter.filterTagged) {
        return {
            enabled,
            queryKey: getUntaggedTransactionsQueryKey({ path, query }),
            queryFn: async ({ signal }: { signal: AbortSignal }) => {
                const { data, headers } = await getUntaggedTransactions({ path, query, signal, throwOnError: true });
                return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
            },
        };
    }

    return {
        enabled,
        queryKey: getTransactionsQueryKey({ path, query }),
        queryFn: async ({ signal }: { signal: AbortSignal }) => {
            const { data, headers } = await getTransactions({ path, query, signal, throwOnError: true });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
        },
    };
};

export const useTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    const { enabled, queryKey, queryFn } = transactionsQueryConfig(accountId, filter, pageSize, pageNumber, sortField, sortDirection);

    return useQuery({ queryKey, queryFn, enabled });
};

// Warms the active transactions query into the cache. Used by the transactions route loader so a
// hover-preload (or the click) fetches the list ahead of the component mounting. No-op if the
// filter has no date range (the query would be disabled anyway).
export const warmTransactions = (queryClient: QueryClient, accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    const { enabled, queryKey, queryFn } = transactionsQueryConfig(accountId, filter, pageSize, pageNumber, sortField, sortDirection);

    if (!enabled) return Promise.resolve(undefined);

    return queryClient.ensureQueryData({ queryKey, queryFn });
};
