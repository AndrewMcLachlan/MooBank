import { useQuery } from "@tanstack/react-query";
import { runRetirementProjection } from "api/sdk.gen";
import { retirementProjectionQueryKey } from "./keys";

/**
 * The server-side projection run, modelled as a real query so the result is cached rather than
 * recalculated on every render. Plan mutations invalidate
 * {@link retirementProjectionQueryKey}, which re-runs it for active consumers.
 */
export const useRetirementProjection = (planId: string) =>
    useQuery({
        queryKey: retirementProjectionQueryKey(planId),
        queryFn: async ({ signal }) => {
            const { data } = await runRetirementProjection({ path: { planId }, signal, throwOnError: true });
            return data;
        },
        enabled: !!planId,
        staleTime: 5 * 60 * 1000,
    });
