import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction } from "api/types.gen";
import type { TransactionUpdate } from "helpers/transactions";
import { State } from "store/state";
import { toast } from "react-toastify";
import {
    getTransactionsQueryKey,
    updateTransactionMutation,
} from "api/@tanstack/react-query.gen";
import { buildTransactionsQueryKey } from "./transactionKeys";

export const useUpdateTransaction = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutateAsync, ...rest } = useMutation({
        ...updateTransactionMutation(),
        onMutate: (variables) => {

            const queryKey = buildTransactionsQueryKey((variables as any).path!.instrumentId, filter, pageSize, currentPage, sortField, sortDirection);
            const transactions = { ...queryClient.getQueryData<PagedResult<Transaction>>(queryKey) };
            if (!transactions?.results) return;

            const transaction = transactions.results.find(tr => tr.id === (variables as any).path!.id);
            if (!transaction) return;

            const body = variables.body as TransactionUpdate;
            transaction.notes = body.notes;
            transaction.splits = body.splits;
            transaction.excludeFromReporting = body.excludeFromReporting;
            transaction.tags = body.splits.flatMap(s => s.tags);

            queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);

        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    return {
        mutateAsync: (accountId: string, transactionId: string, transaction: TransactionUpdate) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId, id: transactionId } } as any), { pending: "Updating transaction", success: "Transaction updated", error: "Failed to update transaction" }),
        ...rest,
    };
};
