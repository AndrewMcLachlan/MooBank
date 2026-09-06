import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { growthStrategyRatesOptions, growthStrategyRatesQueryKey, saveGrowthStrategyRateMutation } from "api/@tanstack/react-query.gen";

/**
 * The assumed return for each investment strategy, lowest first — the risk ladder, with Custom at
 * the front because it carries no rate of its own.
 */
export const useGrowthStrategyRates = () => useQuery(growthStrategyRatesOptions());

export const useSaveGrowthStrategyRate = () => {
    const queryClient = useQueryClient();

    return useMutation({
        ...saveGrowthStrategyRateMutation(),
        onSettled: () => queryClient.invalidateQueries({ queryKey: growthStrategyRatesQueryKey() }),
    });
};
