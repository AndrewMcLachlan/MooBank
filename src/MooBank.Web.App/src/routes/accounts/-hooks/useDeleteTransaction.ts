import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import { toast } from "@andrewmclachlan/moo-ds";
import type { Transaction } from "api/types.gen";
import { deleteTransactionMutation } from "api/@tanstack/react-query.gen";
import { useTransactionSearch } from "../-transactions/hooks/useTransactionSearch";
import { buildTransactionsQueryKey, invalidateAccountViews, invalidateTransactionLists } from "./transactionKeys";

export const useDeleteTransaction = () => {

    const queryClient = useQueryClient();

    const { filter, page, pageSize, sortField, sortDirection } = useTransactionSearch();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteTransactionMutation(),
        onMutate: async (variables) => {

            const queryKey = buildTransactionsQueryKey((variables as any).path!.instrumentId, filter, pageSize, page, sortField, sortDirection);
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<PagedResult<Transaction>>(queryKey);
            if (!previous?.results) return { queryKey, previous: undefined };

            const id = (variables as any).path!.id;
            const next: PagedResult<Transaction> = {
                ...previous,
                results: previous.results.filter(tr => tr.id !== id),
                total: previous.total - 1,
            };
            queryClient.setQueryData<PagedResult<Transaction>>(queryKey, next);

            return { queryKey, previous };
        },
        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(context.queryKey, context.previous);
            }
        },
        // The balance view is derived from the transaction history, so it moves with the delete.
        onSettled: () => Promise.all([invalidateTransactionLists(queryClient), invalidateAccountViews(queryClient)]),
    });

    return {
        mutateAsync: (accountId: string, transactionId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, id: transactionId } } as any), { pending: "Deleting transaction", success: "Transaction deleted", error: "Failed to delete transaction" }),
        ...rest,
    };
};
