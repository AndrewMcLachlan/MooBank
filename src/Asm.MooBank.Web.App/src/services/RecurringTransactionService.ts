import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import type { RecurringTransaction } from "api/types.gen";
import {
    getRecurringTransactionsForAVirtualAccountOptions,
    getRecurringTransactionsForAVirtualAccountQueryKey,
    createRecurringTransactionMutation,
    updateRecurringTransactionMutation,
    deleteRecurringTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useGetRecurringTransactions = (accountId: string, virtualAccountId: string) =>
    useQuery({
        ...getRecurringTransactionsForAVirtualAccountOptions({ path: { accountId, virtualAccountId } }),
    });

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
