import { useQueryClient, useMutation } from "@tanstack/react-query";
import { emptyGuid, toast } from "@andrewmclachlan/moo-ds";
import type { RecurringTransaction, RecurringTransactionDetails } from "api/types.gen";
import {
    getRecurringTransactionsQueryKey,
    createRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useCreateRecurringTransaction = (instrumentId: string, virtualInstrumentId: string) => {

    const queryClient = useQueryClient();

    const queryKey = getRecurringTransactionsQueryKey({ path: { instrumentId, virtualInstrumentId } });

    const { mutateAsync } = useMutation({
        ...createRecurringTransactionMutation(),
        onMutate: async (variables) => {
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                // The server assigns the id, so the optimistic row is the submitted details
                // plus the ids we already know.
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, [...previous, {
                    ...variables.body,
                    id: emptyGuid,
                    virtualInstrumentId,
                }]);
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

    const create = (recurringTransaction: RecurringTransactionDetails) => {

        toast.promise(mutateAsync({ body: recurringTransaction, path: { instrumentId, virtualInstrumentId } }), { pending: "Creating recurring transaction", success: "Recurring transaction created", error: "Failed to create recurring transaction" });
    };

    return create;

}
