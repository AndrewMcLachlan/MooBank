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
        onMutate: async (variables) => {
            const queryKey = getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } });
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, previous.filter(rt => rt.id !== (variables as any).path.recurringTransactionId));
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

    const deleteRecurringTransaction = (recurringTransactionId: string) => {

        mutate({ path: { accountId, recurringTransactionId } });
    }

    return deleteRecurringTransaction;
}
