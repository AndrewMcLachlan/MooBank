import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    updateBillAccountMutation,
    getBillAccountQueryKey,
    getBillAccountsQueryKey,
    getBillAccountSummariesByTypeQueryKey,
} from "api/@tanstack/react-query.gen";
import type { UpdateBillAccount } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateBillAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateBillAccountMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBillAccountQueryKey({ path: { instrumentId: variables.path.instrumentId } }) });
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getBillAccountSummariesByTypeQueryKey() });
            // The per-type lists are keyed by utility type, which the edit does not know; an
            // id-only partial key matches every one of them.
            queryClient.invalidateQueries({ queryKey: [{ _id: "getBillAccountsByType" }] });
        },
    });

    return {
        mutateAsync: (id: string, account: UpdateBillAccount) =>
            toast.promise(mutateAsync({ body: account, path: { instrumentId: id } }), { pending: "Saving account", success: "Account saved", error: "Failed to save account" }),
        ...rest,
    };
};
