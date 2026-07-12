import { useMutation, useQueryClient } from "@tanstack/react-query";
import { setBalanceMutation } from "api/@tanstack/react-query.gen";
import type { CreateTransaction } from "models/transactions";
import { toast } from "@andrewmclachlan/moo-ds";
import { invalidateTransactionLists, invalidateAccountViews } from "./transactionKeys";

export const useUpdateBalance = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...setBalanceMutation(),
        onSettled: () => {
            invalidateTransactionLists(queryClient);
            invalidateAccountViews(queryClient);
        },
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Updating balance", success: "Balance updated", error: "Failed to update balance" }),
        ...rest,
    };
};
