import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createPlannedItemMutation, getForecastPlanQueryKey } from "api/@tanstack/react-query.gen";
import type { PlannedItem } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { forecastResultQueryKey } from "./keys";

export const useCreatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...createPlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const create = (planId: string, item: Partial<PlannedItem>) => {
        toast.promise(mutateAsync({ body: item as any, path: { planId } }), { pending: "Creating planned item", success: "Planned item created", error: "Failed to create planned item" });
    };

    return { create, isPending };
};
