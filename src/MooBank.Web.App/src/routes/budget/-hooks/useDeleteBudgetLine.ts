import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteBudgetLineMutation, getBudgetQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...deleteBudgetLineMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
        },
    });

    const deleteBudgetLine = (year: number, lineId: string) => {
        toast.promise(mutateAsync({ path: { year, id: lineId } }), { pending: "Deleting budget line", success: "Budget line deleted", error: "Failed to delete budget line" });
    };

    return deleteBudgetLine;
};
