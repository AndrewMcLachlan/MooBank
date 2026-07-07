import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getForecastPlanQueryKey, updatePlannedItemMutation } from "api/@tanstack/react-query.gen";
import type { PlannedItem } from "api/types.gen";
import { forecastResultQueryKey } from "./keys";

export const useUpdatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...updatePlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const update = (planId: string, itemId: string, item: Partial<PlannedItem>) => {
        mutate({ body: item as any, path: { planId, itemId } });
    };

    return { update, isPending };
};
