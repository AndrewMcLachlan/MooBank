import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { InstitutionAccount, accountId, AccountList, ImportAccount } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { ListItem } from "models/ListItem";

export const accountsKey = "accounts";
export const formattedAccountsKey = "formatted-accounts";
export const accountListKey = "account-list";

export const useAccounts = (): UseQueryResult<InstitutionAccount[]> => useApiGet<InstitutionAccount[]>([accountsKey], `api/accounts`);

export const useFormattedAccounts = () => useApiGet<AccountList>([formattedAccountsKey], "api/accounts/position");

export const useAccountsList = () => useApiGet<ListItem<string>[]>([accountListKey], "api/accounts/list");

export const useAccount = (accountId: string) => useApiGet<InstitutionAccount>(["account", { accountId }], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPost<InstitutionAccount, null, InstitutionAccount>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: InstitutionAccount) => {
        mutate([null, account]);
    };

    return create;
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<InstitutionAccount, accountId, InstitutionAccount>((accountId) => `api/accounts/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: ["account", { accountId }]});
        }
    });

    const update = (account: InstitutionAccount) => {
        mutate([account.id, account]);
    };

    return update;
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<InstitutionAccount, accountId, { currentBalance: number, availableBalance: number }>((accountId) => `api/accounts/${accountId}/balance`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        },
    });

    const update = (accountId: string, currentBalance: number, availableBalance: number) => {

        mutate([accountId, {currentBalance, availableBalance}]);
    };

    return update;
}
