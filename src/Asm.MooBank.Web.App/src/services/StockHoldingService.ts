import { UseMutationResult, useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { InstitutionAccount, accountId, AccountList, ImportAccount, NewStockHolding, StockHolding } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { accountsKey } from "./AccountService";
import { UseCreateMutationResult, UseUpdateMutationResult } from "./Mutations";

export const stockKey = "stock";

export const useStockHolding = (accountId: string) => useApiGet<StockHolding>([stockKey, { accountId }], `api/stock/${accountId}`);

export const useCreateStockHolding = (): UseCreateMutationResult<(account: NewStockHolding) => void> => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<InstitutionAccount, null, NewStockHolding>(() => `api/stock`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    const create = (account: NewStockHolding) => {
        mutate([null, account]);
    };

    return { create, ...rest };
}

export const useUpdateStockHolding = (): UseUpdateMutationResult<(account: StockHolding) => void> => {
    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<InstitutionAccount, accountId, InstitutionAccount>((accountId) => `api/stock/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: [stockKey, { accountId }]});
        }
    });

    const update = (account: StockHolding) => {
        mutate([account.id, account]);
    };

    return { update, ...rest };
}
