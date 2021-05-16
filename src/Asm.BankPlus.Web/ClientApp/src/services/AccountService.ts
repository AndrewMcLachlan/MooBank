import { useQueryClient } from "react-query";
import { Account, accountId, Accounts, ImportAccount } from "../models";
import { useApiGet, useApiPost } from "./api";
import { useApiPatch } from "./useApiPatch";

export const accountsKey = "accounts";

export const useAccounts = () => useApiGet<Accounts>(accountsKey, `api/accounts`);

export const useAccount = (accountId: string) => useApiGet<Account>(["account", { accountId }], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<Account, null, { account: Account, importAccount: ImportAccount }>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries(accountsKey);
        }
    });

    const create = (account: Account, importAccount: ImportAccount) => {
        mutate([null, {account, importAccount}]);
    };

    return { create, ...rest };
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<Account, accountId, Account>((accountId) => `api/accounts/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries(accountsKey);
            queryClient.invalidateQueries(["account", { accountId }]);
        }
    });

    const update = (account: Account) => {
        mutate([account.id, account]);
    };

    return { update, ...rest };
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<Account, accountId, { currentBalance: number, availableBalance: number }>((accountId) => `api/accounts/${accountId}/balance`, {
        onSettled: () => {
            queryClient.invalidateQueries(accountsKey);
        },
    });

    const update = (accountId: string, currentBalance: number, availableBalance: number) => {

        mutate([accountId, {currentBalance, availableBalance}]);
    };

    return { update, ...rest };
}