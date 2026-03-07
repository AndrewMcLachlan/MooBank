import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateStockHoldingMutation, getStockHoldingQueryKey, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { StockHolding } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateStockHolding = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateStockHoldingMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getStockHoldingQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        },
    });

    return {
        mutateAsync: (account: StockHolding) =>
            toast.promise(mutateAsync({ body: account as any, path: { instrumentId: account.id } } as any), { pending: "Updating shares", success: "Shares updated", error: "Failed to update shares" }),
        ...rest
    };
}
