import { useApiDelete, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { useQueryClient } from "@tanstack/react-query";
import { VirtualAccount } from "models";
import { RecurringTransaction } from "models/RecurringTransaction";

export const useCreateRecurringTransaction = () => {

    const queryClient = useQueryClient();

    const useResult = useApiPost<RecurringTransaction, { accountId: string, virtualAccountId: string }, RecurringTransaction>(({ accountId }) => `api/accounts/${accountId}/recurring`, {
        onMutate: async ([{ accountId, virtualAccountId }, recurringTransaction]) => {

            const virtualAccount = queryClient.getQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }]);

            virtualAccount.recurringTransactions.push(recurringTransaction);

            queryClient.setQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], virtualAccount);

        },
        onSuccess: async (_responseData, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }] });
        }
    });

    const create = (accountId: string, virtualAccountId: string, recurringTransaction: RecurringTransaction) => {

        useResult.mutate([{ accountId, virtualAccountId }, recurringTransaction]);
    };

    return create;

}

export const useUpdateRecurringTransaction = () => {

    const queryClient = useQueryClient();

    const useResult = useApiPatch<RecurringTransaction, { accountId: string, virtualAccountId: string, recurringTransactionId: string }, RecurringTransaction>(({ accountId, recurringTransactionId }) => `api/accounts/${accountId}/recurring/${recurringTransactionId}`, {
        onMutate: async ([{ accountId, virtualAccountId, recurringTransactionId }, recurringTransaction]) => {

            const virtualAccount = queryClient.getQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }]);

            virtualAccount.recurringTransactions = virtualAccount.recurringTransactions.map(rt => rt.id === recurringTransactionId ? recurringTransaction : rt);

            queryClient.setQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], virtualAccount);

        },
        onSuccess: async (_responseData, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }] });
        }
    });

    const update = (accountId: string, virtualAccountId: string, recurringTransaction: RecurringTransaction) => {

        useResult.mutate([{ accountId, virtualAccountId, recurringTransactionId: recurringTransaction.id }, recurringTransaction]);
    };

    return update;

}

export const useDeleteRecurringTransaction = () => {

    const queryClient = useQueryClient();

    const useResult = useApiDelete<{ accountId: string; virtualAccountId: string, recurringTransactionId: string }>(({ accountId, recurringTransactionId }) => `api/accounts/${accountId}/recurring/${recurringTransactionId}`, {
        onSuccess: (_data, { accountId, virtualAccountId, recurringTransactionId }) => {
            
            const virtualAccount = queryClient.getQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }]);

            virtualAccount.recurringTransactions = virtualAccount.recurringTransactions.filter(rt => rt.id !== recurringTransactionId);
            queryClient.setQueryData<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], virtualAccount);
        }
    });

    const deleteRecurringTransaction = (accountId: string, virtualAccountId: string, recurringTransactionId: string) => {
            
            useResult.mutate({ accountId, virtualAccountId, recurringTransactionId });
    }

    return deleteRecurringTransaction;
}
