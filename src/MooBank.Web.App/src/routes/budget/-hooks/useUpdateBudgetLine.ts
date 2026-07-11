import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateBudgetLineMutation, getAllBudgetYearsQueryKey, getBudgetQueryKey } from "api/@tanstack/react-query.gen";
import type { BudgetLine } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateBudgetLineMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
            queryClient.invalidateQueries({ queryKey: getAllBudgetYearsQueryKey() });
        },
    });

    const update = (year: number, budget: BudgetLine) => {
        toast.promise(mutateAsync({ body: budget, path: { year, id: budget.id } }), { pending: "Updating budget line", success: "Budget line updated", error: "Failed to update budget line" });
    };

    return update;
};
