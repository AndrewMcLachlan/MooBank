import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { CreateLogicalAccount, CreateTransaction, LogicalAccount, InstrumentId, CreateInstitutionAccount, InstitutionAccount, UpdateInstitutionAccount } from "../models";
import { toast } from "react-toastify";

export const accountsKey = "accounts";

export const useAccounts = (): UseQueryResult<LogicalAccount[]> => useApiGet<LogicalAccount[]>([accountsKey], `api/accounts`);

export const useAccount = (accountId: string) => useApiGet<LogicalAccount>([accountsKey, accountId], `api/accounts/${accountId}`);

export const useCreateAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<LogicalAccount, null, CreateLogicalAccount>(() => `api/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (account: CreateLogicalAccount) =>
            toast.promise(mutateAsync([null, account]), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
}

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<LogicalAccount, InstrumentId, LogicalAccount>((accountId) => `api/accounts/${accountId}`, {
        onSettled: (_data, _error, [accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [accountsKey, accountId] });
        }
    });

    return {
        mutateAsync: (account: LogicalAccount) =>
            toast.promise(mutateAsync([account.id, account]), { pending: "Updating account", success: "Account updated", error: "Failed to update account" }),
        ...rest,
    };
}

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<LogicalAccount, InstrumentId, CreateTransaction>((accountId) => `api/accounts/${accountId}/transactions/balance-adjustment`, {
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

export const useCreateInstitutionAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, InstrumentId, CreateInstitutionAccount>((accountId) => `api/accounts/${accountId}/institution-accounts`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccount: CreateInstitutionAccount) =>
            toast.promise(mutateAsync([accountId, institutionAccount]), { pending: "Creating institution account", success: "Institution account created", error: "Failed to create institution account" }),
        ...rest,
    };
}

export const useUpdateInstitutionAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<InstitutionAccount, [InstrumentId, string], UpdateInstitutionAccount>(([accountId, institutionAccountId]) => `api/accounts/${accountId}/institution-accounts/${institutionAccountId}`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccountId: string, institutionAccount: UpdateInstitutionAccount) =>
            toast.promise(mutateAsync([[accountId, institutionAccountId], institutionAccount]), { pending: "Updating institution account", success: "Institution account updated", error: "Failed to update institution account" }),
        ...rest,
    };
}

export const useCloseInstitutionAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, [InstrumentId, string]>(([accountId, institutionAccountId]) => `api/accounts/${accountId}/institution-accounts/${institutionAccountId}/close`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccountId: string) =>
            toast.promise(mutateAsync([[accountId, institutionAccountId], null]), { pending: "Closing institution account", success: "Institution account closed", error: "Failed to close institution account" }),
        ...rest,
    };
}