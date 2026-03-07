import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createForecastPlanMutation, getAllForecastPlansQueryKey } from "api/@tanstack/react-query.gen";
import type { ForecastPlan } from "api/types.gen";

export const useCreateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, isPending } = useMutation({
        ...createForecastPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const create = (plan: Partial<ForecastPlan>) => {
        mutate({ body: plan as any });
    };

    const createAsync = (plan: Partial<ForecastPlan>) => {
        return mutateAsync({ body: plan as any });
    };

    return { create, createAsync, isPending };
};
