import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createStockHoldingMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { CreateStock } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateStockHolding = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createStockHoldingMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateStock) =>
            toast.promise(mutateAsync({ body: account }), { pending: "Creating shares", success: "Shares created", error: "Failed to create shares" }),
        ...rest
    };
}
