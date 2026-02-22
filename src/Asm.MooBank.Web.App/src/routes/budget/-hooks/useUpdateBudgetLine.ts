import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateBudgetLineMutation, getAllBudgetYearsQueryKey } from "api/@tanstack/react-query.gen";
import type { BudgetLine } from "api/types.gen";

export const useUpdateBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateBudgetLineMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllBudgetYearsQueryKey() });
        },
    });

    const update = (year: number, budget: BudgetLine) => {
        mutate({ body: budget, path: { year, id: budget.id } });
    };

    return update;
};
