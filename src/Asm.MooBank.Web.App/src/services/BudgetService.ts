import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { Budget, BudgetLine, BudgetReportByMonth, BudgetReportForMonthBreakdown, BudgetReportValueMonth } from "../models";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

interface BudgetVariables {
    year: number,
    lineId?: string,
}

export const budgetKey = ["budget"];

export const useBudgetYears = (): UseQueryResult<number[]> => useApiGet<number[]>([budgetKey], `api/budget`);

export const useBudget = (year: number) => useApiGet<Budget>([budgetKey, year], `api/budget/${year}`);

export const useCreateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/budget/${variables.year}/lines`, {
        onSettled: (_data, _error, [variables, _line]) => {
            queryClient.invalidateQueries({ queryKey: [budgetKey, variables.year] });
        }
    });

    const create = (year: number, budgetLine: BudgetLine) => {
        mutate([{ year }, budgetLine]);
    };

    return create;
}

export const useUpdateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/budget/${variables.year}/lines/${variables.lineId}`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [budgetKey] });
        }
    });

    const update = (year: number, budget: BudgetLine) => {
        mutate([{ year, lineId: budget.id }, budget]);
    };

    return update;
}


export const useDeleteBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiDelete<BudgetVariables>((variables) => `api/budget/${variables.year}/lines/${variables.lineId}`, {
        onSuccess: (_data, variables: BudgetVariables) => {
            queryClient.invalidateQueries({ queryKey: [budgetKey, variables.year] });
            /*const budget = queryClient.getQueryData<Budget>([budgetKey, variables.year]);
            if (!budget) return;
            budget.expensesLines = budget.expensesLines.filter(r => r.id !== (variables.lineId));
            budget.incomeLines = budget.incomeLines.filter(r => r.id !== (variables.lineId));
            queryClient.setQueryData<Budget>([budgetKey, variables.year], {...budget});*/
        }
    });

    const deleteBudgetLine = (year: number, lineId: string) => {
        mutate({ year, lineId });
    }

    return deleteBudgetLine;
}

export const useGetTagValue = (tagId: number) => useApiGet<number>(["budget", "by-tag", tagId], `api/budget/tag/${tagId}`, {
    enabled: !!tagId
});

export const useBudgetReport = (year: number) => useApiGet<BudgetReportByMonth>([budgetKey, year, "report"], `api/budget/${year}/report`);

export const useBudgetReportForMonth = (year: number, month: number) => useApiGet<BudgetReportValueMonth>([budgetKey, year, "report", month], `api/budget/${year}/report/${month + 1}`);

export const useBudgetReportForMonthBreakdown = (year: number, month: number) => useApiGet<BudgetReportForMonthBreakdown>([budgetKey, year, "report", month], `api/budget/${year}/report/${month + 1}/breakdown`);

export const useBudgetReportForMonthBreakdownUnbudgeted = (year: number, month: number) => useApiGet<BudgetReportForMonthBreakdown>([budgetKey, year, "report", month, "unbudgeted"], `api/budget/${year}/report/${month + 1}/breakdown/unbudgeted`);
