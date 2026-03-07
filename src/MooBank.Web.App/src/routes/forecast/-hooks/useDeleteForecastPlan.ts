import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteForecastPlanMutation, getAllForecastPlansQueryKey } from "api/@tanstack/react-query.gen";

export const useDeleteForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deleteForecastPlanMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const deletePlan = (planId: string) => {
        mutate({ path: { id: planId } });
    };

    return deletePlan;
};
