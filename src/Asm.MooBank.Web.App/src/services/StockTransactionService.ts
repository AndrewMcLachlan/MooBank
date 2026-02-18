import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import * as Models from "../models";
import { TransactionsFilter } from "../store/state";
import { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import { toast } from "react-toastify";
import {
    getStockTransactionsOptions,
    getStockTransactionsQueryKey,
    createStockTransactionMutation,
} from "api/@tanstack/react-query.gen";
import {
    SortDirection as GenSortDirection,
    CreateStockTransactionData,
} from "api/types.gen";

export const useStockTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    return useQuery({
        ...getStockTransactionsOptions({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: {
                Filter: filter.description || undefined,
                Start: filter.start || undefined,
                End: filter.end || undefined,
                SortField: sortField || undefined,
                SortDirection: (sortDirection || "Descending") as GenSortDirection,
            },
        }),
        select: (data) => data as unknown as PagedResult<Models.StockTransaction>,
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

    const create = (accountId: string, transaction: Models.CreateStockTransaction) => {
        toast.promise(mutateAsync({ body: transaction as unknown as CreateStockTransactionData["body"], path: { instrumentId: accountId } }), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" });
    };

    return create;
}
