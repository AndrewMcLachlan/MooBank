import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "@andrewmclachlan/moo-ds";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsQueryKey,
    updateRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useUpdateRecurringTransaction = (instrumentId: string, virtualInstrumentId: string) => {

    const queryClient = useQueryClient();

    const queryKey = getRecurringTransactionsQueryKey({ path: { instrumentId, virtualInstrumentId } });

    const { mutateAsync } = useMutation({
        ...updateRecurringTransactionMutation(),
        onMutate: async (variables) => {
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                const { recurringTransactionId } = variables.path;
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, previous.map(rt => rt.id === recurringTransactionId ? { ...rt, ...variables.body } : rt));
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

    const update = (recurringTransaction: RecurringTransaction) => {

        const { id, description, amount, schedule, nextRun } = recurringTransaction;

        toast.promise(mutateAsync({
            body: { description, amount, schedule, nextRun },
            path: { instrumentId, virtualInstrumentId, recurringTransactionId: id },
        }), { pending: "Updating recurring transaction", success: "Recurring transaction updated", error: "Failed to update recurring transaction" });
    };

    return update;

}
