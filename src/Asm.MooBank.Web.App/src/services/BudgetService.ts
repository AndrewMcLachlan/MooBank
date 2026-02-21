import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getAllBudgetYearsOptions,
    getBudgetOptions,
    getBudgetQueryKey,
    getAllBudgetYearsQueryKey,
    createBudgetLineMutation,
    updateBudgetLineMutation,
    deleteBudgetLineMutation,
    getBudgetAmountForTagOptions,
    getBudgetReportOptions,
    getBudgetReportForMonthOptions,
    getBudgetReportBreakdownForMonthOptions,
    getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions,
} from "api/@tanstack/react-query.gen";
import type { Budget, BudgetLine } from "api/types.gen";

export const useBudgetYears = () => useQuery({ ...getAllBudgetYearsOptions() });

export const useBudget = (year: number) => useQuery({
    ...getBudgetOptions({ path: { year } }),
});

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

export const useGetTagValue = (tagId: number) => useQuery({
    ...getBudgetAmountForTagOptions({ path: { tagId } }),
    enabled: !!tagId,
});

export const useBudgetReport = (year: number) => useQuery({ ...getBudgetReportOptions({ path: { year } }) });

export const useBudgetReportForMonth = (year: number, month: number) => useQuery({
    ...getBudgetReportForMonthOptions({ path: { year, month: month + 1 } }),
});

export const useBudgetReportForMonthBreakdown = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthOptions({ path: { year, month: month + 1 } }),
});

export const useBudgetReportForMonthBreakdownUnbudgeted = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions({ path: { year, month: month + 1 } }),
});
