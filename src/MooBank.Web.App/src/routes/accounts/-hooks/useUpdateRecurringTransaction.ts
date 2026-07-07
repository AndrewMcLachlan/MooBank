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
            const queryKey = getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } });
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                const updatedTransaction = variables.body as unknown as RecurringTransaction;
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, previous.map(rt => rt.id === updatedTransaction.id ? updatedTransaction : rt));
            }

            return { previous };
        },
        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } }), context.previous);
            }
        },
        onSettled: () => {
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
