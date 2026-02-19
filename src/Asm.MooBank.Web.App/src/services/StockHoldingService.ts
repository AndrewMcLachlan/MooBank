import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getStockHoldingOptions,
    getStockHoldingQueryKey,
    getStockHoldingCpiAdjustedGainLossOptions,
    stockValueReportOptions,
    createStockHoldingMutation,
    updateStockHoldingMutation,
} from "api/@tanstack/react-query.gen";
import type { StockHolding, CreateStock } from "api/types.gen";
import { accountsQueryKey } from "./AccountService";
import { formatISODate } from "helpers/dateFns";
import { toast } from "react-toastify";

export const stockQueryKey = getStockHoldingQueryKey;

// Preserve old export name for cross-service consumers
export const stockKey = "stock";

export const useStockHolding = (accountId: string) => useQuery({
    ...getStockHoldingOptions({ path: { instrumentId: accountId } }),
});

export const useStockHoldingAdjustedGainLoss = (accountId: string) => useQuery({ ...getStockHoldingCpiAdjustedGainLossOptions({ path: { instrumentId: accountId } }) });

export const useCreateStockHolding = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createStockHoldingMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateStock) =>
            toast.promise(mutateAsync({ body: account }), { pending: "Creating shares", success: "Shares created", error: "Failed to create shares" }),
        ...rest
    };
}

export const useUpdateStockHolding = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateStockHoldingMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getStockHoldingQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        },
    });

    return {
        mutateAsync: (account: StockHolding) =>
            toast.promise(mutateAsync({ body: account as any, path: { instrumentId: account.id } } as any), { pending: "Updating shares", success: "Shares updated", error: "Failed to update shares" }),
        ...rest
    };
}

export const useStockValueReport = (accountId: string, start?: Date, end?: Date) => useQuery({
    ...stockValueReportOptions({ path: { instrumentId: accountId }, query: { Start: start ? formatISODate(start) : "", End: end ? formatISODate(end) : "" } }),
    enabled: (!!start && !!end),
});
