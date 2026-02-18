import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getAssetOptions, getAssetQueryKey, createAssetMutation, updateAssetMutation } from "api/@tanstack/react-query.gen";
import type { Asset, CreateAsset } from "api/types.gen";
import { accountsQueryKey } from "./AccountService";
import { toast } from "react-toastify";

export const assetQueryKey = getAssetQueryKey;

export const useAsset = (accountId: string) => useQuery({
    ...getAssetOptions({ path: { id: accountId } }),
});

export const useCreateAsset = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createAssetMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
        },
    });

    return {
        mutateAsync: (asset: CreateAsset) =>
            toast.promise(mutateAsync({ body: asset }), { pending: "Creating asset", success: "Asset created", error: "Failed to create asset" }),
        ...rest,
    };
}

export const useUpdateAsset = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateAssetMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getAssetQueryKey({ path: { id: (variables as any).path!.id } }) });
        },
    });

    return {
        mutateAsync: (account: Asset) =>
            toast.promise(mutateAsync({ body: account as any, path: { id: account.id } } as any), { pending: "Updating asset", success: "Asset updated", error: "Failed to update asset" }),
        ...rest,
    };
}
