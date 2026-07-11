import { useMutation, useQueryClient } from "@tanstack/react-query";
import { generateBudgetMutation, getBudgetQueryKey, getAllBudgetYearsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useGenerateBudget = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...generateBudgetMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
            queryClient.invalidateQueries({ queryKey: getAllBudgetYearsQueryKey() });
        },
    });

    const generate = (year: number) =>
        toast.promise(mutateAsync({ path: { year } }), { pending: "Generating budget", success: "Budget generated", error: "Failed to generate budget" });

    return { generate, generateAsync: mutateAsync, isPending };
};
