import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "@andrewmclachlan/moo-ds";
import {
    getForecastPlanQueryKey,
    getPaymentCandidatesOptions,
    setPlannedItemPaymentsMutation,
} from "api/@tanstack/react-query.gen";
import { forecastResultQueryKey } from "./keys";

/**
 * The payments worth considering for a planned item: spending carrying its tag, in its direction,
 * within two months either side of its own date. The tag cannot say which payment is the item's —
 * one category covers several projects — so it only narrows what is offered.
 */
export const usePaymentCandidates = (planId: string, itemId: string, enabled: boolean) =>
    useQuery({
        ...getPaymentCandidatesOptions({ path: { planId, itemId } }),
        enabled: enabled && !!planId && !!itemId,
    });

export const useSetPlannedItemPayments = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...setPlannedItemPaymentsMutation(),
        onSettled: (_data, _error, variables) => {
            // Links decide what the item has actually cost, so the forecast itself changes.
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const setPayments = (planId: string, itemId: string, transactionIds: string[]) =>
        toast.promise(
            mutateAsync({ body: { transactionIds }, path: { planId, itemId } }),
            { pending: "Linking payments", success: "Payments linked", error: "Failed to link payments" });

    return { setPayments, isPending };
};
