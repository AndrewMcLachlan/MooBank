import { createFileRoute } from "@tanstack/react-router";
import { Transactions } from "../-transactions/Transactions";
import {
    defaultSortDirection,
    defaultSortField,
    getStoredPageSize,
    resolveTransactionSearch,
    searchToFilter,
    validateTransactionSearch,
} from "../-transactions/transactionSearch";
import { warmTransactions } from "../-hooks/useTransactions";
import { getRouterQueryClient } from "utils/routerQueryClient";

export const Route = createFileRoute("/accounts/$id/transactions")({
    validateSearch: validateTransactionSearch,
    loaderDeps: ({ search }) => search,
    // Warm the transaction list into the cache during route resolution. With defaultPreload:"intent"
    // and the account rows' hover-preload, this starts the fetch before the click, so the list is
    // ready (or in flight) by the time the component mounts. Fire-and-forget: never block navigation.
    loader: ({ params, deps }) => {
        const queryClient = getRouterQueryClient();
        if (!queryClient) return;
        const resolved = resolveTransactionSearch(deps as Record<string, unknown>);
        void warmTransactions(
            queryClient,
            params.id,
            searchToFilter(resolved),
            getStoredPageSize(),
            resolved.page ?? 1,
            resolved.sortField ?? defaultSortField,
            resolved.sortDirection ?? defaultSortDirection,
        );
    },
    component: Transactions,
});
