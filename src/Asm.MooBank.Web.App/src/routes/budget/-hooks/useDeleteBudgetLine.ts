import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteBudgetLineMutation, getBudgetQueryKey } from "api/@tanstack/react-query.gen";

export const useDeleteBudgetLine = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deleteBudgetLineMutation(),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: getBudgetQueryKey({ path: { year: variables.path!.year } }) });
        },
    });

    const deleteBudgetLine = (year: number, lineId: string) => {
        mutate({ path: { year, id: lineId } });
    };

    return deleteBudgetLine;
};
