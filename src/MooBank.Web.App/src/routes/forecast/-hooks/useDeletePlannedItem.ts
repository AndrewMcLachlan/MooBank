import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deletePlannedItemMutation, getForecastPlanQueryKey } from "api/@tanstack/react-query.gen";
import { forecastResultQueryKey } from "./keys";

export const useDeletePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deletePlannedItemMutation(),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const deleteItem = (planId: string, itemId: string) => {
        mutate({ path: { planId, itemId } });
    };

    return deleteItem;
};
