import type { QueryClient } from "@tanstack/react-query";
import type { SortDirection } from "@andrewmclachlan/moo-ds";
import type { TransactionFilterType, SortDirection as GenSortDirection } from "api/types.gen";
import type { TransactionsFilter } from "models/transactions";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";

/**
 * Invalidates every cached transaction list query (all accounts, pages, filters and sorts).
 *
 * Generated query keys are a single-element array of `{ _id, baseURL, path, query }`, so an
 * id-only partial key matches all of them via TanStack Query's partial deep matching.
 */
export const invalidateTransactionLists = (queryClient: QueryClient) =>
    Promise.all([
        queryClient.invalidateQueries({ queryKey: [{ _id: "getTransactions" }] }),
        queryClient.invalidateQueries({ queryKey: [{ _id: "getUntaggedTransactions" }] }),
    ]);

/**
 * Invalidates the account-derived views a transaction mutation affects: the accounts list, the
 * single-account query (balance, last transaction) and the in/out period report. Uses the same
 * id-only partial-key matching as the transaction lists.
 */
export const invalidateAccountViews = (queryClient: QueryClient) =>
    Promise.all([
        queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() }),
        queryClient.invalidateQueries({ queryKey: [{ _id: "getAccount" }] }),
        queryClient.invalidateQueries({ queryKey: [{ _id: "inOutReport" }] }),
    ]);

export const buildTransactionsQueryKey = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {
    if (filter.filterTagged) {
        return getUntaggedTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: {
                Filter: filter.description || undefined,
                Start: filter.start || undefined,
                End: filter.end || undefined,
                TagIds: filter.tags,
                SortField: sortField || undefined,
                TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
                SortDirection: (sortDirection || "Descending") as GenSortDirection,
                ExcludeNetZero: filter.filterNetZero || undefined,
            },
        });
    }
    return getTransactionsQueryKey({
        path: { instrumentId: accountId, pageSize, pageNumber },
        query: {
            Filter: filter.description || undefined,
            Start: filter.start || undefined,
            End: filter.end || undefined,
            TagIds: filter.tags,
            SortField: sortField || undefined,
            TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
            SortDirection: (sortDirection || "Descending") as GenSortDirection,
            ExcludeNetZero: filter.filterNetZero || undefined,
        },
    });
};
