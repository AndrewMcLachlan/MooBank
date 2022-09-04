import { useQueryClient } from "react-query";
import { emptyGuid } from "../helpers";
import { Account, accountId,  Accounts,  VirtualAccount } from "../models";
import { accountsKey } from "./AccountService";
import { useApiGet, useApiPost } from "./api";
import { useApiPatch } from "./useApiPatch";

interface VirtualAccountVariables {
    accountId: accountId;
    virtualAccountId: accountId;
}

export const useVirtualAccount = (accountId: accountId, virtualAccountId: accountId) => useApiGet<Account>(["virtualaccount", { accountId, virtualAccountId }], `api/accounts/${accountId}/virtual/${virtualAccountId}`);

export const useCreateVirtualAccount = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest } = useApiPost<VirtualAccount, accountId, VirtualAccount>((accountId) => `api/accounts/${accountId}/virtual`, {
        onSuccess: () => {
            queryClient.invalidateQueries(accountsKey);
        }
    });

    const create = (accountId: accountId, virtualAccount: VirtualAccount) => {
        mutate([accountId, virtualAccount]);
    };

    return { create, ...rest };
}

export const useUpdateVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest } = useApiPatch<Account, VirtualAccountVariables, VirtualAccount>(({ accountId, virtualAccountId }) => `api/accounts/${accountId}/virtual/${virtualAccountId}`, {
        onSettled: (_data,_error, [{accountId, virtualAccountId}]) => {
            queryClient.invalidateQueries(accountsKey);
            queryClient.invalidateQueries(["virtualaccount", { accountId, virtualAccountId }]);
        }
    });

    const update = (accountId: accountId, account: VirtualAccount) => {
        mutate([{ accountId, virtualAccountId: account.id }, account]);
    };

    return { update, ...rest };
}

export const useUpdateVirtualAccountBalance = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest } = useApiPatch<VirtualAccount, VirtualAccountVariables, { balance: number }>(({ accountId, virtualAccountId }) => `api/accounts/${accountId}/virtual/${virtualAccountId}/balance`, {

        onMutate: async ([{ accountId, virtualAccountId }, { balance }]) => {

            await queryClient.cancelQueries(accountsKey);

            const accounts = queryClient.getQueryData<Accounts>(accountsKey);
            
            if (!accounts) return;

            const account = accounts.accounts.find(a => a.id === accountId);
            const vAccount = account.virtualAccounts.find(a => a.id === virtualAccountId);
            const remaining = account.virtualAccounts.find(a => a.id === emptyGuid);
            //const others = account.virtualAccounts.filter(a => a.id !== emptyGuid);
            
            const difference = vAccount.balance - balance;

            vAccount.balance = balance;
            remaining.balance += difference;
            account.virtualAccountRemainingBalance = remaining.balance;
            //accounts.accounts = [];
            accounts.position = 0;
            
            queryClient.setQueryData<Accounts>(accountsKey, { ...accounts });

            console.debug(queryClient.getQueryData<Accounts>(accountsKey).accounts.find(a => a.id === accountId));

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: () => {
            queryClient.invalidateQueries(accountsKey);
        },
    });

    const update = (accountId: string, virtualAccountId: accountId, balance: number) => {

        mutate([{ accountId, virtualAccountId }, { balance }]);
    };

    return { update, ...rest };
}