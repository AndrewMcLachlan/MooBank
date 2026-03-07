import { SortDirection } from "@andrewmclachlan/moo-ds";
import type { TransactionFilterType, SortDirection as GenSortDirection } from "api/types.gen";
import { TransactionsFilter } from "store/state";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
} from "api/@tanstack/react-query.gen";

export const buildTransactionsQueryKey = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {
    if (filter.filterTagged) {
        return getUntaggedTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
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
