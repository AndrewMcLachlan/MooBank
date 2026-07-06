import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateStockTransaction } from "models/stocks";
import { toast } from "@andrewmclachlan/moo-ds";
import { createStockTransactionMutation } from "api/@tanstack/react-query.gen";

export const useCreateStockTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createStockTransactionMutation(),
        onSettled: () => {
            // Id-only partial key matches every cached getStockTransactions query (all pages and instruments).
            queryClient.invalidateQueries({ queryKey: [{ _id: "getStockTransactions" }] });
        }
    });

    const create = (accountId: string, transaction: CreateStockTransaction) => {
        toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" });
    };

    return create;
}
