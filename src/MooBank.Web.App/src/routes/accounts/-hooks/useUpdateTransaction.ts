import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction } from "api/types.gen";
import type { TransactionUpdate } from "models/transactions";
import type { State } from "store/state";
import { toast } from "@andrewmclachlan/moo-ds";
import { updateTransactionMutation } from "api/@tanstack/react-query.gen";
import { buildTransactionsQueryKey, invalidateTransactionLists } from "./transactionKeys";

export const useUpdateTransaction = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutateAsync, ...rest } = useMutation({
        ...updateTransactionMutation(),
        onMutate: async (variables) => {

            const queryKey = buildTransactionsQueryKey((variables as any).path!.instrumentId, filter, pageSize, currentPage, sortField, sortDirection);
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<PagedResult<Transaction>>(queryKey);
            if (!previous?.results) return { queryKey, previous: undefined };

            const body = variables.body as TransactionUpdate;
            const next: PagedResult<Transaction> = {
                ...previous,
                results: previous.results.map(tr => tr.id === (variables as any).path!.id ? {
                    ...tr,
                    notes: body.notes,
                    splits: body.splits,
                    excludeFromReporting: body.excludeFromReporting,
                    tags: body.splits.flatMap(s => s.tags),
                } : tr),
            };
            queryClient.setQueryData<PagedResult<Transaction>>(queryKey, next);

            return { queryKey, previous };
        },
        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(context.queryKey, context.previous);
            }
        },
        onSettled: () => invalidateTransactionLists(queryClient),
    });

    return {
        mutateAsync: (accountId: string, transactionId: string, transaction: TransactionUpdate) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId, id: transactionId } } as any), { pending: "Updating transaction", success: "Transaction updated", error: "Failed to update transaction" }),
        ...rest,
    };
};
