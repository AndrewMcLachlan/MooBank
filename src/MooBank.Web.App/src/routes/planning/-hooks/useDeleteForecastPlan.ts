import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteForecastPlanMutation, getAllForecastPlansQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...deleteForecastPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const deletePlan = (planId: string) => {
        toast.promise(mutateAsync({ path: { id: planId } }), { pending: "Deleting forecast plan", success: "Forecast plan deleted", error: "Failed to delete forecast plan" });
    };

    return deletePlan;
};
