import { UseQueryResult, useQueryClient, } from "@tanstack/react-query";
import { InstitutionAccount, InstrumentId, NewAsset, Asset } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { accountsKey } from "./AccountService";

export const assetKey = "asset";

export const useAsset = (accountId: string): UseQueryResult<Asset> => useApiGet<Asset>([assetKey, { accountId }], `api/assets/${accountId}`);

export const useCreateAsset = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<InstitutionAccount, null, NewAsset>(() => `api/assets`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: NewAsset) => {
        mutate([null, account]);
    };

    return create;
}

export const useUpdateAsset = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<Asset, InstrumentId, Asset>((accountId) => `api/assets/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: [assetKey, { accountId }]});
        }
    });

    const update = (account: Asset) => {
        mutate([account.id, account]);
    };

    return update;
}
