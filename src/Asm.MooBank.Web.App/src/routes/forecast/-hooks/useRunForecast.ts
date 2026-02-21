import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getForecastPlanQueryKey, runForecastMutation } from "api/@tanstack/react-query.gen";
import type { ForecastResult } from "api/types.gen";
import { forecastKey } from "./keys";

export const useRunForecast = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, data, isPending } = useMutation({
        ...runForecastMutation(),
        onSuccess: (_data, variables) => {
            queryClient.setQueryData(
                getForecastPlanQueryKey({ path: { id: variables.path!.planId } }),
                undefined,
            );
            queryClient.setQueryData(
                [...forecastKey, variables.path!.planId, "result"],
                _data,
            );
        },
    });

    const run = (planId: string) => {
        mutate({ path: { planId } });
    };

    const runAsync = (planId: string) => {
        return mutateAsync({ path: { planId } });
    };

    return { run, runAsync, result: data as ForecastResult | undefined, isPending };
};
