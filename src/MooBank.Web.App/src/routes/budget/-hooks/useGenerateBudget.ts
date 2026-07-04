import { useMutation, useQueryClient } from "@tanstack/react-query";
import { generateBudgetMutation, getBudgetQueryKey, getAllBudgetYearsQueryKey } from "api/@tanstack/react-query.gen";

export const useGenerateBudget = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, isPending } = useMutation({
        ...generateBudgetMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
            queryClient.invalidateQueries({ queryKey: getAllBudgetYearsQueryKey() });
        },
    });

    const generate = (year: number) => mutate({ path: { year } });

    return { generate, generateAsync: mutateAsync, isPending };
};
