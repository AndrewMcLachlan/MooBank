import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllForecastPlansQueryKey, updateForecastPlanMutation } from "api/@tanstack/react-query.gen";
import type { ForecastPlan } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { forecastResultQueryKey } from "./keys";

export const useUpdateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...updateForecastPlanMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
            queryClient.invalidateQueries({ queryKey: forecastResultQueryKey(variables.path!.id) });
        },
    });

    const update = (planId: string, plan: Partial<ForecastPlan>) => {
        toast.promise(mutateAsync({ body: plan as any, path: { id: planId } }), { pending: "Updating forecast plan", success: "Forecast plan updated", error: "Failed to update forecast plan" });
    };

    return { update, isPending };
};
