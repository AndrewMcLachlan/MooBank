import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateTransaction } from "models/transactions";
import { toast } from "react-toastify";
import {
    getTransactionsQueryKey,
    createTransactionMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";

export const useCreateTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createTransactionMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
            queryClient.refetchQueries({ queryKey: getAccountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" }),
        ...rest,
    };
}
