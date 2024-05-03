import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { CreateTransaction, InstitutionAccount, InstrumentId, AccountList, ImportAccount, CreateInstitutionAccount } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { ListItem } from "models/ListItem";

export const accountsKey = "accounts";
export const formattedAccountsKey = "formatted-accounts";
export const accountListKey = "account-list";

export const useAccounts = (): UseQueryResult<InstitutionAccount[]> => useApiGet<InstitutionAccount[]>([accountsKey], `api/accounts`);

export const useFormattedAccounts = () => useApiGet<AccountList>([formattedAccountsKey], "api/instruments/summary");

export const useAccountsList = () => useApiGet<ListItem<string>[]>([accountListKey], "api/instruments/list");

export const useAccount = (accountId: string) => useApiGet<InstitutionAccount>(["account", { accountId }], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPost<InstitutionAccount, null, CreateInstitutionAccount>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: CreateInstitutionAccount) => {
        mutate([null, account]);
    };

    return create;
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<InstitutionAccount, InstrumentId, InstitutionAccount>((accountId) => `api/accounts/${accountId}`, {
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

    const { mutate} = useApiPost<InstitutionAccount, InstrumentId, CreateTransaction>((accountId) => `api/accounts/${accountId}/transactions/balance-adjustment`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        },
    });

    const update = (accountId: string, transaction: CreateTransaction) => {

        mutate([accountId, transaction]);
    };

    return update;
}
