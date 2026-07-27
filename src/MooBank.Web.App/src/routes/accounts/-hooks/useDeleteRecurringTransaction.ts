import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "@andrewmclachlan/moo-ds";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsQueryKey,
    deleteRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useDeleteRecurringTransaction = (instrumentId: string, virtualInstrumentId: string) => {

    const queryClient = useQueryClient();

    const queryKey = getRecurringTransactionsQueryKey({ path: { instrumentId, virtualInstrumentId } });

    const { mutateAsync } = useMutation({
        ...deleteRecurringTransactionMutation(),
        onMutate: async (variables) => {
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, previous.filter(rt => rt.id !== variables.path.recurringTransactionId));
            }

            return { previous };
        },
        onError: (_error, _variables, context) => {
            if (context?.previous) {
                queryClient.setQueryData(queryKey, context.previous);
            }
        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey });
        },
    });

    const deleteRecurringTransaction = (recurringTransactionId: string) => {

        toast.promise(mutateAsync({ path: { instrumentId, virtualInstrumentId, recurringTransactionId } }), { pending: "Deleting recurring transaction", success: "Recurring transaction deleted", error: "Failed to delete recurring transaction" });
    }

    return deleteRecurringTransaction;
}
