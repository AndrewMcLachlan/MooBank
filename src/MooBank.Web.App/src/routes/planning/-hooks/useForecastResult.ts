import { useQuery } from "@tanstack/react-query";
import { runForecast } from "api/sdk.gen";
import { forecastResultQueryKey } from "./keys";

/**
 * The server-side forecast run, modelled as a real query so the result is cached and shared
 * between the dashboard widget and the forecast page. Plan and planned-item mutations
 * invalidate {@link forecastResultQueryKey}, which triggers a re-run for active consumers.
 */
export const useForecastResult = (planId: string) =>
    useQuery({
        queryKey: forecastResultQueryKey(planId),
        queryFn: async ({ signal }) => {
            const { data } = await runForecast({ path: { planId }, signal, throwOnError: true });
            return data;
        },
        enabled: !!planId,
        staleTime: 5 * 60 * 1000,
    });
