import { useQueryClient, useMutation } from "@tanstack/react-query";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsForAVirtualAccountQueryKey,
    deleteRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useDeleteRecurringTransaction = (accountId: string, virtualAccountId: string) => {

    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deleteRecurringTransactionMutation(),
        onSuccess: (_data, variables) => {

            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } })
            );

            const newRecurringTransactions = recurringTransactions.filter(rt => rt.id !== variables.path.recurringTransactionId);
            queryClient.setQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }),
                newRecurringTransactions
            );
        },
    });

    const deleteRecurringTransaction = (recurringTransactionId: string) => {

        mutate({ path: { accountId, recurringTransactionId } });
    }

    return deleteRecurringTransaction;
}
