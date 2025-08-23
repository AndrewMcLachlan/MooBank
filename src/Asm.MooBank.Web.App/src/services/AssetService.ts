import { UseQueryResult, useQueryClient, } from "@tanstack/react-query";
import { InstitutionAccount, InstrumentId, NewAsset, Asset } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { accountsKey } from "./AccountService";
import { toast } from "react-toastify";

export const assetKey = "asset";

export const useAsset = (accountId: string): UseQueryResult<Asset> => useApiGet<Asset>([assetKey, { accountId }], `api/assets/${accountId}`);

export const useCreateAsset = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, null, NewAsset>(() => `api/assets`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    return {
        mutateAsync: (asset: NewAsset) =>
            toast.promise(mutateAsync([null, asset]), { pending: "Creating asset", success: "Asset created", error: "Failed to create asset" }),
        ...rest,
    };
}

export const useUpdateAsset = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<Asset, InstrumentId, Asset>((accountId) => `api/assets/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: [assetKey, { accountId }]});
        }
    });

    return {
        mutateAsync: (account: Asset) =>
            toast.promise(mutateAsync([account.id, account]), { pending: "Updating asset", success: "Asset updated", error: "Failed to update asset" }),
        ...rest,
    };
}
