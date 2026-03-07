import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateStockTransaction } from "models/stocks";
import { toast } from "react-toastify";
import {
    getStockTransactionsQueryKey,
    createStockTransactionMutation,
} from "api/@tanstack/react-query.gen";

export const useCreateStockTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createStockTransactionMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getStockTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    const create = (accountId: string, transaction: CreateStockTransaction) => {
        toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" });
    };

    return create;
}
