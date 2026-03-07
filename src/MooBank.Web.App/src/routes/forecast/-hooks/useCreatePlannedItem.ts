import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createPlannedItemMutation, getForecastPlanQueryKey } from "api/@tanstack/react-query.gen";
import type { PlannedItem } from "api/types.gen";
import { forecastKey } from "./keys";

export const useCreatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...createPlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.planId, "result"] });
        },
    });

    const create = (planId: string, item: Partial<PlannedItem>) => {
        mutate({ body: item as any, path: { planId } });
    };

    return { create, isPending };
};
