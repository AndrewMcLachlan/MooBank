import { useQueryClient, useMutation } from "@tanstack/react-query";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsForAVirtualAccountQueryKey,
    createRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateRecurringTransaction = (accountId: string, virtualAccountId: string) => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
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

        toast.promise(mutateAsync({ body: recurringTransaction as any, path: { accountId } } as any), { pending: "Creating recurring transaction", success: "Recurring transaction created", error: "Failed to create recurring transaction" });
    };

    return create;

}
