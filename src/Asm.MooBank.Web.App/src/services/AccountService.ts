import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { CreateInstitutionAccount, CreateTransaction, InstitutionAccount, InstrumentId } from "../models";
import { toast } from "react-toastify";

export const accountsKey = "accounts";

export const useAccounts = (): UseQueryResult<InstitutionAccount[]> => useApiGet<InstitutionAccount[]>([accountsKey], `api/accounts`);

export const useAccount = (accountId: string) => useApiGet<InstitutionAccount>([accountsKey, accountId], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, null, CreateInstitutionAccount>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (account: CreateInstitutionAccount) =>
            toast.promise(mutateAsync([null, account]), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<InstitutionAccount, InstrumentId, InstitutionAccount>((accountId) => `api/accounts/${accountId}`, {
        onSettled: (_data, _error, [accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [accountsKey, accountId] });
        }
    });

    return {
        mutateAsync: (account: InstitutionAccount) =>
            toast.promise(mutateAsync([account.id, account]), { pending: "Updating account", success: "Account updated", error: "Failed to update account" }),
        ...rest,
    };
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, InstrumentId, CreateTransaction>((accountId) => `api/accounts/${accountId}/transactions/balance-adjustment`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        },
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync([accountId, transaction]), { pending: "Updating balance", success: "Balance updated", error: "Failed to update balance" }),
        ...rest,
    };
}
