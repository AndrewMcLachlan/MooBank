import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deletePlannedItemMutation, getForecastPlanQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { forecastResultQueryKey } from "./keys";

export const useDeletePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...deletePlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.planId) });
        },
    });

    const deleteItem = (planId: string, itemId: string) => {
        toast.promise(mutateAsync({ path: { planId, itemId } }), { pending: "Deleting planned item", success: "Planned item deleted", error: "Failed to delete planned item" });
    };

    return deleteItem;
};
