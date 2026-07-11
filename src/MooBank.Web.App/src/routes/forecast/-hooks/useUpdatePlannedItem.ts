import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getForecastPlanQueryKey, updatePlannedItemMutation } from "api/@tanstack/react-query.gen";
import type { PlannedItem } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { forecastResultQueryKey } from "./keys";

export const useUpdatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...updatePlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const update = (planId: string, itemId: string, item: Partial<PlannedItem>) => {
        toast.promise(mutateAsync({ body: item as any, path: { planId, itemId } }), { pending: "Updating planned item", success: "Planned item updated", error: "Failed to update planned item" });
    };

    return { update, isPending };
};
