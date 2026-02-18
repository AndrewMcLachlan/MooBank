import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import type { StockTransaction } from "api/types.gen";
import type { CreateStockTransaction } from "helpers/stocks";
import { TransactionsFilter } from "../store/state";
import { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import { toast } from "react-toastify";
import {
    getStockTransactionsQueryKey,
    createStockTransactionMutation,
} from "api/@tanstack/react-query.gen";
import { getStockTransactions } from "api/sdk.gen";
import {
    SortDirection as GenSortDirection,
} from "api/types.gen";

export const useStockTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    return useQuery({
        queryKey: getStockTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: {
                Filter: filter.description || undefined,
                Start: filter.start || undefined,
                End: filter.end || undefined,
                SortField: sortField || undefined,
                SortDirection: (sortDirection || "Descending") as GenSortDirection,
            },
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getStockTransactions({
                path: { instrumentId: accountId, pageSize, pageNumber },
                query: {
                    Filter: filter.description || undefined,
                    Start: filter.start || undefined,
                    End: filter.end || undefined,
                    SortField: sortField || undefined,
                    SortDirection: (sortDirection || "Descending") as GenSortDirection,
                },
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<StockTransaction>;
        },
    });
}

export const useCreateStockTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createStockTransactionMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getStockTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    const create = (accountId: string, transaction: CreateStockTransaction) => {
        toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" });
    };

    return create;
}
