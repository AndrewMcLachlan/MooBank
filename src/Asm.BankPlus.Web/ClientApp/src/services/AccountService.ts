import { useQueryClient } from "react-query";
import { Account, accountId, Accounts, ImportAccount } from "../models";
import { useApiGet, useApiPost } from "./api";
import { useApiPatch } from "./useApiPatch";

export const useAccounts = () => useApiGet<Accounts>(["accounts"], `api/accounts`);

export const useAccount = (accountId: string) => useApiGet<Account>(["accounts", {id: accountId }], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<Account, null, { account: Account, importAccount: ImportAccount }>(() => `api/accounts`, {
        onMutate: ([, account]) => {
            const accounts = queryClient.getQueryData<Accounts>(["accounts"]);

            accounts.accounts.push(account.account);

            queryClient.setQueryData<Accounts>(["accounts"], accounts);
        },
        onSuccess: () => {
            queryClient.invalidateQueries(["accounts"]);
        }
    });

    const create = (account: Account, importAccount: ImportAccount) => {
        mutate([null, {account, importAccount}]);
    };

    return { create, ...rest };
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<Account, accountId, { currentBalance: number, availableBalance: number }>((accountId) => `api/accounts/${accountId}`, {
        onError: () => {
            queryClient.invalidateQueries(["accounts"]);
        },
    });

    const update = (accountId: string, currentBalance: number, availableBalance: number) => {

        mutate([accountId, {currentBalance, availableBalance}]);
    };

    return { update, ...rest };
}