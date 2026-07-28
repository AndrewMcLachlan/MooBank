import { useQuery } from "@tanstack/react-query";
import { runRetirementProjection } from "api/sdk.gen";
import type { RetirementProjectionOverrides } from "api/types.gen";
import { retirementProjectionQueryKey } from "./keys";

/**
 * The server-side projection run, modelled as a real query so the result is cached rather than
 * recalculated on every render. Plan mutations invalidate
 * {@link retirementProjectionQueryKey}, which re-runs it for active consumers.
 *
 * Overrides form part of the query key, so moving a tweak slider fetches its own result and moving
 * it back is served from cache. They are never persisted; saving is a separate, deliberate act.
 */
export const useRetirementProjection = (planId: string, overrides?: RetirementProjectionOverrides) =>
    useQuery({
        queryKey: retirementProjectionQueryKey(planId, overrides),
        queryFn: async ({ signal }) => {
            const { data } = await runRetirementProjection({ path: { planId }, body: overrides ?? null, signal, throwOnError: true });
            return data;
        },
        enabled: !!planId,
        staleTime: 5 * 60 * 1000,
        // Hold the previous curve on screen while a slider settles, so the chart does not blank out
        // between movements.
        placeholderData: (previous) => previous,
    });
