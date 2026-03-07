import { useQueryClient, useMutation } from "@tanstack/react-query";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsForAVirtualAccountQueryKey,
    createRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useCreateRecurringTransaction = (accountId: string, virtualAccountId: string) => {

    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...createRecurringTransactionMutation(),
        onMutate: async (variables) => {

            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } })
            );

            recurringTransactions.push(variables.body as unknown as RecurringTransaction);

            queryClient.setQueryData<RecurringTransaction[]>(
                getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }),
                recurringTransactions
            );
        },
        onSuccess: async () => {
            queryClient.invalidateQueries({
                queryKey: getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }),
            });
        },
    });

    const create = (recurringTransaction: RecurringTransaction) => {

        mutate({ body: recurringTransaction as any, path: { accountId } } as any);
    };

    return create;

}
