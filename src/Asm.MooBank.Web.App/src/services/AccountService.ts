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
import type { LogicalAccount, CreateInstitutionAccount, UpdateInstitutionAccount, CreateAccount } from "api/types.gen";
import type { CreateTransaction } from "helpers/transactions";
import { toast } from "react-toastify";

export const accountsQueryKey = getAccountsQueryKey;

// Preserve old export name for cross-service consumers
export const accountsKey = "accounts";

export const useAccounts = () => useQuery({
    ...getAccountsOptions(),
});

export const useAccount = (accountId: string) => useQuery({
    ...getAccountOptions({ path: { instrumentId: accountId } }),
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
        mutateAsync: (account: CreateAccount) =>
            toast.promise(mutateAsync({ body: account }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateAccountMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getAccountQueryKey({ path: { instrumentId: (variables as any).path!.id } }) });
        },
    });

    return {
        mutateAsync: (account: LogicalAccount) =>
            toast.promise(mutateAsync({ body: account, path: { id: account.id } } as any), { pending: "Updating account", success: "Account updated", error: "Failed to update account" }),
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
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Updating balance", success: "Balance updated", error: "Failed to update balance" }),
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
        mutateAsync: (accountId: string, institutionAccount: CreateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount, path: { instrumentId: accountId } }), { pending: "Creating institution account", success: "Institution account created", error: "Failed to create institution account" }),
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
        mutateAsync: (accountId: string, institutionAccountId: string, institutionAccount: UpdateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount, path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Updating institution account", success: "Institution account updated", error: "Failed to update institution account" }),
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
        mutateAsync: (accountId: string, institutionAccountId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Closing institution account", success: "Institution account closed", error: "Failed to close institution account" }),
        ...rest,
    };
};
