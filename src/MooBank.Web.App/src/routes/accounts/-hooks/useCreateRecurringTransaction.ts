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
            const queryKey = getRecurringTransactionsForAVirtualAccountQueryKey({ path: { accountId, virtualAccountId } });
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<RecurringTransaction[]>(queryKey);
            if (previous) {
                queryClient.setQueryData<RecurringTransaction[]>(queryKey, [...previous, variables.body as unknown as RecurringTransaction]);
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

    const create = (recurringTransaction: RecurringTransaction) => {

        mutate({ body: recurringTransaction as any, path: { accountId } } as any);
    };

    return create;

}
