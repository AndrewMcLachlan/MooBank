import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateTransaction } from "models/transactions";
import { toast } from "@andrewmclachlan/moo-ds";
import {
    createTransactionMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { invalidateTransactionLists } from "./transactionKeys";

export const useCreateTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createTransactionMutation(),
        onSettled: () => {
            invalidateTransactionLists(queryClient);
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" }),
        ...rest,
    };
}
