import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllForecastPlansQueryKey, updateForecastPlanMutation } from "api/@tanstack/react-query.gen";
import type { ForecastPlan } from "api/types.gen";
import { forecastKey } from "./keys";

export const useUpdateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...updateForecastPlanMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.id, "result"] });
        },
    });

    const update = (planId: string, plan: Partial<ForecastPlan>) => {
        mutate({ body: plan as any, path: { id: planId } });
    };

    return { update, isPending };
};
