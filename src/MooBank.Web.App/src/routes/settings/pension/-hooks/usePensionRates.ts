import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { pensionRatesOptions, pensionRatesQueryKey, savePensionRatesMutation } from "api/@tanstack/react-query.gen";

/**
 * The recorded sets of Age Pension rates, newest first — so the first is the one in force.
 */
export const usePensionRates = () => useQuery(pensionRatesOptions());

export const useSavePensionRates = () => {
    const queryClient = useQueryClient();

    return useMutation({
        ...savePensionRatesMutation(),
        onSettled: () => queryClient.invalidateQueries({ queryKey: pensionRatesQueryKey() }),
    });
};
