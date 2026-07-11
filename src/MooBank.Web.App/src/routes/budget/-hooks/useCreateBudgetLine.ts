import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createBudgetLineMutation, getBudgetQueryKey } from "api/@tanstack/react-query.gen";
import type { BudgetLine } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createBudgetLineMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
        },
    });

    const create = (year: number, budgetLine: BudgetLine) => {
        toast.promise(mutateAsync({ body: budgetLine, path: { year } }), { pending: "Creating budget line", success: "Budget line created", error: "Failed to create budget line" });
    };

    return create;
};
