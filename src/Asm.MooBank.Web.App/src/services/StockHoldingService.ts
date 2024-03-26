import { UseQueryResult, useQueryClient, } from "@tanstack/react-query";
import { InstitutionAccount, AccountId, NewStockHolding, StockHolding } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { accountsKey } from "./AccountService";

export const stockKey = "stock";

export const useStockHolding = (accountId: string): UseQueryResult<StockHolding> => useApiGet<StockHolding>([stockKey, { accountId }], `api/stock/${accountId}`);

export const useCreateStockHolding = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<InstitutionAccount, null, NewStockHolding>(() => `api/stock`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: NewStockHolding) => {
        mutate([null, account]);
    };

    return create;
}

export const useUpdateStockHolding = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<StockHolding, AccountId, StockHolding>((accountId) => `api/stock/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: [stockKey, { accountId }]});
        }
    });

    const update = (account: StockHolding) => {
        mutate([account.id, account]);
    };

    return update;
}
