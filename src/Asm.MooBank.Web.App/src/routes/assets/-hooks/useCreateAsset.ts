import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createAssetMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { CreateAsset } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateAsset = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createAssetMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (asset: CreateAsset) =>
            toast.promise(mutateAsync({ body: asset }), { pending: "Creating asset", success: "Asset created", error: "Failed to create asset" }),
        ...rest,
    };
}
