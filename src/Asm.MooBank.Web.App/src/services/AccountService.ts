import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { CreateInstitutionAccount, CreateTransaction, InstitutionAccount, InstrumentId } from "../models";

export const accountsKey = "accounts";

export const useAccounts = (): UseQueryResult<InstitutionAccount[]> => useApiGet<InstitutionAccount[]>([accountsKey], `api/accounts`);

export const useAccount = (accountId: string) => useApiGet<InstitutionAccount>([accountsKey, accountId], `api/accounts/${accountId}`);

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
            queryClient.invalidateQueries({ queryKey: [accountsKey, accountId ]});
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
