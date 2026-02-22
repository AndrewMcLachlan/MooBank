import { useMutation, useQueryClient } from "@tanstack/react-query";
import { setBalanceMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { CreateTransaction } from "models/transactions";
import { toast } from "react-toastify";

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
