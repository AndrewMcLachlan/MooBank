import { useQueryClient, useMutation } from "@tanstack/react-query";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsForAVirtualAccountQueryKey,
    updateRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useUpdateRecurringTransaction = (accountId: string, virtualAccountId: string) => {

    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateRecurringTransactionMutation(),
        onMutate: async (variables) => {

            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } })
            );

            const updatedTransaction = variables.body as unknown as RecurringTransaction;
            const newRecurringTransactions = recurringTransactions.map(rt => rt.id === updatedTransaction.id ? updatedTransaction : rt);

            queryClient.setQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }),
                newRecurringTransactions
            );
        },
        onSuccess: async () => {
            queryClient.invalidateQueries({
                queryKey: getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }),
            });
        },
    });

    const update = (recurringTransaction: RecurringTransaction) => {

        mutate({ body: recurringTransaction as any, path: { accountId, recurringTransactionId: recurringTransaction.id } } as any);
    };

    return update;

}
