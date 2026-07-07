import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateBudgetLineMutation, getAllBudgetYearsQueryKey, getBudgetQueryKey } from "api/@tanstack/react-query.gen";
import type { BudgetLine } from "api/types.gen";

export const useUpdateBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateBudgetLineMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
            queryClient.invalidateQueries({ queryKey: getAllBudgetYearsQueryKey() });
        },
    });

    const update = (year: number, budget: BudgetLine) => {
        mutate({ body: budget, path: { year, id: budget.id } });
    };

    return update;
};
