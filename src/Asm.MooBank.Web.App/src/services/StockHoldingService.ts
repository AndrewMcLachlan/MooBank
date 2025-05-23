import { UseQueryResult, useQueryClient, } from "@tanstack/react-query";
import { InstitutionAccount, InstrumentId, NewStockHolding, StockHolding } from "../models";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { accountsKey } from "./AccountService";
import { StockValueReport } from "models/stock-holding/StockValueReport";
import { formatISODate } from "helpers/dateFns";
import { toast } from "react-toastify";

export const stockKey = "stock";

export const useStockHolding = (accountId: string): UseQueryResult<StockHolding> => useApiGet<StockHolding>([stockKey, { accountId }], `api/stocks/${accountId}`);

export const useStockHoldingAdjustedGainLoss = (accountId: string): UseQueryResult<number> => useApiGet<number>([stockKey, { accountId }, "adjusted-gain-loss"], `api/stocks/${accountId}/cpi-adjusted-gain-loss`);

export const useCreateStockHolding = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<InstitutionAccount, null, NewStockHolding>(() => `api/stocks`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
        }
    });

    return {
        mutateAsync: (account: NewStockHolding) =>
            toast.promise(mutateAsync([null, account]), { pending: "Creating shares", success: "Shares created", error: "Failed to create shares" }),
        ...rest
    };
}

export const useUpdateStockHolding = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<StockHolding, InstrumentId, StockHolding>((accountId) => `api/stocks/${accountId}`, {
        onSettled: (_data,_error,[accountId]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey]});
            queryClient.invalidateQueries({ queryKey: [stockKey, { accountId }]});
        }
    });

    return {
        mutateAsync: (account: StockHolding) =>
            toast.promise(mutateAsync([account.id, account]), { pending: "Updating shares", success: "Shares updated", error: "Failed to update shares" }),
        ...rest
    };
}

export const useStockValueReport = (accountId: string, start?: Date, end?: Date) => useApiGet<StockValueReport>([stockKey, accountId, "value", start, end], `api/stocks/${accountId}/reports/value?start=${start && formatISODate(start)}&end=${start && formatISODate(end)}`, { enabled: (!!start && !!end) });
