import { useQueryClient } from "react-query";
import { emptyGuid } from "helpers";
import { Account, accountId,  AccountList,  VirtualAccount } from "models";
import { accountListKey, accountsKey } from "./AccountService";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

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
            await queryClient.cancelQueries(accountListKey);

            const accounts = queryClient.getQueryData<AccountList>([accountListKey]);
            
            if (!accounts) return;

            const account = accounts.accountGroups.flatMap(g => g.accounts).find(a => a.id === accountId);
            if (!account) return;
            const vAccount = account.virtualAccounts.find(a => a.id === virtualAccountId);
            if (!vAccount) return;
            const remaining = account.virtualAccounts.find(a => a.id === emptyGuid);
            if (!remaining) return;
            //const others = account.virtualAccounts.filter(a => a.id !== emptyGuid);
            
            const difference = vAccount.balance - balance;

            vAccount.balance = balance;
            remaining.balance += difference;
            account.virtualAccountRemainingBalance = remaining.balance;
            //accounts.accounts = [];
            accounts.position = 0;
            
            queryClient.setQueryData<AccountList>(accountListKey, { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: () => {
            queryClient.invalidateQueries(accountsKey);
            queryClient.invalidateQueries(accountListKey);
        },
    });

    const update = (accountId: string, virtualAccountId: accountId, balance: number) => {

        mutate([{ accountId, virtualAccountId }, { balance }]);
    };

    return { update, ...rest };
}