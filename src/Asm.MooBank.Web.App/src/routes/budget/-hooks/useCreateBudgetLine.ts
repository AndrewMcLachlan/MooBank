import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createBudgetLineMutation, getBudgetQueryKey } from "api/@tanstack/react-query.gen";
import type { BudgetLine } from "api/types.gen";

export const useCreateBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...createBudgetLineMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
        },
    });

    const create = (year: number, budgetLine: BudgetLine) => {
        mutate({ body: budgetLine, path: { year } });
    };

    return create;
};
