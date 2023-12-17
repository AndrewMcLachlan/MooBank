import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { InstitutionAccount, accountId, AccountList, ImportAccount } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

export const accountsKey = "accounts";
export const accountListKey = "account-list";

export const useAccounts = (): UseQueryResult<InstitutionAccount[]> => useApiGet<InstitutionAccount[]>([accountsKey], `api/accounts`);

export const useFormattedAccounts = () => useApiGet<AccountList>([accountListKey], "api/accounts/position");

export const useAccount = (accountId: string) => useApiGet<InstitutionAccount>(["account", { accountId }], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<InstitutionAccount, null, { account: InstitutionAccount, importAccount?: ImportAccount }>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: InstitutionAccount, importAccount?: ImportAccount) => {
        mutate([null, {account, importAccount}]);
    };

    return { create, ...rest };
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<InstitutionAccount, accountId, InstitutionAccount>((accountId) => `api/accounts/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: ["account", { accountId }]});
        }
    });

    const update = (account: InstitutionAccount) => {
        mutate([account.id, account]);
    };

    return { update, ...rest };
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<InstitutionAccount, accountId, { currentBalance: number, availableBalance: number }>((accountId) => `api/accounts/${accountId}/balance`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        },
    });

    const update = (accountId: string, currentBalance: number, availableBalance: number) => {

        mutate([accountId, {currentBalance, availableBalance}]);
    };

    return { update, ...rest };
}