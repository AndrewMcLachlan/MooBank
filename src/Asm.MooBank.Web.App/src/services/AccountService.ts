import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getAccountsOptions,
    getAccountsQueryKey,
    getAccountOptions,
    getAccountQueryKey,
    createAccountMutation,
    updateAccountMutation,
    setBalanceMutation,
    createInstitutionAccountMutation,
    updateInstitutionAccountMutation,
    closeInstitutionAccountMutation,
} from "api/@tanstack/react-query.gen";
import {
    CreateAccount as GenCreateAccount,
    LogicalAccount as GenLogicalAccount,
    CreateInstitutionAccount as GenCreateInstitutionAccount,
    UpdateInstitutionAccount as GenUpdateInstitutionAccount,
    CreateTransaction as GenCreateTransaction,
} from "api/types.gen";
import { CreateLogicalAccount, CreateTransaction, LogicalAccount, InstrumentId, CreateInstitutionAccount, UpdateInstitutionAccount } from "../models";
import { toast } from "react-toastify";

export const accountsQueryKey = getAccountsQueryKey;

// Preserve old export name for cross-service consumers
export const accountsKey = "accounts";

export const useAccounts = () => useQuery({
    ...getAccountsOptions(),
    select: (data) => data as unknown as LogicalAccount[],
});

export const useAccount = (accountId: string) => useQuery({
    ...getAccountOptions({ path: { instrumentId: accountId } }),
    select: (data) => data as unknown as LogicalAccount,
});

export const useCreateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createAccountMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateLogicalAccount) =>
            toast.promise(mutateAsync({ body: account as unknown as GenCreateAccount }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateAccountMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getAccountQueryKey({ path: { instrumentId: variables.path!.id } }) });
        },
    });

    return {
        mutateAsync: (account: LogicalAccount) =>
            toast.promise(mutateAsync({ body: account as unknown as GenLogicalAccount, path: { id: account.id } }), { pending: "Updating account", success: "Account updated", error: "Failed to update account" }),
        ...rest,
    };
};

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...setBalanceMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as unknown as GenCreateTransaction, path: { instrumentId: accountId } }), { pending: "Updating balance", success: "Balance updated", error: "Failed to update balance" }),
        ...rest,
    };
};

export const useCreateInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccount: CreateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount as unknown as GenCreateInstitutionAccount, path: { instrumentId: accountId } }), { pending: "Creating institution account", success: "Institution account created", error: "Failed to create institution account" }),
        ...rest,
    };
};

export const useUpdateInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccountId: string, institutionAccount: UpdateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount as unknown as GenUpdateInstitutionAccount, path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Updating institution account", success: "Institution account updated", error: "Failed to update institution account" }),
        ...rest,
    };
};

export const useCloseInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...closeInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: InstrumentId, institutionAccountId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Closing institution account", success: "Institution account closed", error: "Failed to close institution account" }),
        ...rest,
    };
};
