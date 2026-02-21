import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateAssetMutation, getAssetQueryKey, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { Asset } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateAsset = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateAssetMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getAssetQueryKey({ path: { id: (variables as any).path!.id } }) });
        },
    });

    return {
        mutateAsync: (account: Asset) =>
            toast.promise(mutateAsync({ body: account as any, path: { id: account.id } } as any), { pending: "Updating asset", success: "Asset updated", error: "Failed to update asset" }),
        ...rest,
    };
}
