import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { useQueryClient } from "@tanstack/react-query";
import { VirtualAccount } from "models";
import { RecurringTransaction } from "models/RecurringTransaction";

export const useGetRecurringTransactions = (accountId: string, virtualAccountId: string) => 
    useApiGet<RecurringTransaction[]>(["virtualaccount", {accountId, virtualAccountId}, "recurring"], `api/accounts/${accountId}/virtual/${virtualAccountId}/recurring`);

export const useCreateRecurringTransaction = () => {

    const queryClient = useQueryClient();

    
    const useResult = useApiPost<RecurringTransaction, { accountId: string, virtualAccountId: string }, RecurringTransaction>(({ accountId }) => `api/accounts/${accountId}/recurring`, {
        onMutate: async ([{ accountId, virtualAccountId }, recurringTransaction]) => {

            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }, "recurring"]);

            recurringTransactions.push(recurringTransaction);

            queryClient.setQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }, "recurring"], recurringTransactions);

        },
        onSuccess: async (_responseData, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }, "recurring"] });
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

            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }, "recurring"]);

            const newRecurringTransactions = recurringTransactions.map(rt => rt.id === recurringTransactionId ? recurringTransaction : rt);

            queryClient.setQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }, "recurring"], newRecurringTransactions);

        },
        onSuccess: async (_responseData, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }, "recurring"] });
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
            
            const recurringTransactions = queryClient.getQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }]);

            const newRecurringTransactions = recurringTransactions.filter(rt => rt.id !== recurringTransactionId);
            queryClient.setQueryData<RecurringTransaction[]>(["virtualaccount", { accountId, virtualAccountId }], newRecurringTransactions);
        }
    });

    const deleteRecurringTransaction = (accountId: string, virtualAccountId: string, recurringTransactionId: string) => {
            
            useResult.mutate({ accountId, virtualAccountId, recurringTransactionId });
    }

    return deleteRecurringTransaction;
}
