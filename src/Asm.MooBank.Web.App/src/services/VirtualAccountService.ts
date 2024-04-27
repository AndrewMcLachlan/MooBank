import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { InstitutionAccount, AccountId, AccountList, VirtualAccount } from "../models";
import { formattedAccountsKey, accountsKey } from "./AccountService";
import { emptyGuid, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

interface VirtualAccountVariables {
    accountId: AccountId;
    virtualAccountId: AccountId;
}

export const useVirtualAccounts = (accountId: AccountId): UseQueryResult<VirtualAccount[]> => useApiGet<VirtualAccount[]>(["virtualaccount", accountId], `api/accounts/${accountId}/virtual`);

export const useVirtualAccount = (accountId: AccountId, virtualAccountId: AccountId) => useApiGet<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], `api/accounts/${accountId}/virtual/${virtualAccountId}`);

export const useCreateVirtualAccount = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<VirtualAccount, AccountId, VirtualAccount>((accountId) => `api/accounts/${accountId}/virtual`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    const create = (accountId: AccountId, virtualAccount: VirtualAccount) => {
        mutate([accountId, virtualAccount]);
    };

    return create;
}

export const useUpdateVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<InstitutionAccount, VirtualAccountVariables, VirtualAccount>(({ accountId, virtualAccountId }) => `api/accounts/${accountId}/virtual/${virtualAccountId}`, {
        onSettled: (_data, _error, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }] });
        }
    });

    const update = (accountId: AccountId, account: VirtualAccount) => {
        mutate([{ accountId, virtualAccountId: account.id }, account]);
    };

    return update;
}

export const useUpdateVirtualAccountBalance = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<VirtualAccount, VirtualAccountVariables, { balance: number }>(({ accountId, virtualAccountId }) => `api/accounts/${accountId}/virtual/${virtualAccountId}/balance`, {

        onMutate: async ([{ accountId, virtualAccountId }, { balance }]) => {

            await queryClient.cancelQueries({ queryKey: [accountsKey] });
            await queryClient.cancelQueries({ queryKey: [formattedAccountsKey] });

            const accounts = queryClient.getQueryData<AccountList>([formattedAccountsKey]);

            if (!accounts) return;

            const account = accounts.groups.flatMap(g => g.accounts).find(a => a.id === accountId);
            if (!account) return;
            const vAccount = account.virtualAccounts.find(a => a.id === virtualAccountId);
            if (!vAccount) return;
            const remaining = account.virtualAccounts.find(a => a.id === emptyGuid);
            if (!remaining) return;
            //const others = account.virtualAccounts.filter(a => a.id !== emptyGuid);

            const difference = vAccount.currentBalance - balance;

            vAccount.currentBalance = balance;
            remaining.currentBalance += difference;
            account.virtualAccountRemainingBalance = remaining.currentBalance;
            //accounts.accounts = [];
            accounts.position = 0;

            queryClient.setQueryData<AccountList>([formattedAccountsKey], { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [formattedAccountsKey] });
        },
    });

    const update = (accountId: string, virtualAccountId: AccountId, balance: number) => {

        mutate([{ accountId, virtualAccountId }, { balance }]);
    };

    return update;
}
