import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createForecastPlanMutation, getAllForecastPlansQueryKey } from "api/@tanstack/react-query.gen";
import type { ForecastPlan } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...createForecastPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const createAsync = (plan: Partial<ForecastPlan>) => {
        return toast.promise(mutateAsync({ body: plan as any }), { pending: "Creating forecast plan", success: "Forecast plan created", error: "Failed to create forecast plan" });
    };

    const create = (plan: Partial<ForecastPlan>) => {
        createAsync(plan);
    };

    return { create, createAsync, isPending };
};
