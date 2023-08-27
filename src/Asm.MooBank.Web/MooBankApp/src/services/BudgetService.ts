import { useQueryClient } from "react-query";
import { Budget, BudgetLine } from "../models";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

interface BudgetVariables {
    year: number,
    lineId?: string,
}

export const budgetKey = "budget";

export const useBudgetYears = () => useApiGet<number[]>([budgetKey], `api/budget`);

export const useBudget = (year: number) => useApiGet<Budget>([budgetKey, year], `api/budget/${year}`);

export const useCreateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/budget/${variables.year}/lines`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const create = (year: number, budgetLine: BudgetLine) => {
        mutate([{year}, budgetLine]);
    };

    return { create, ...rest };
}

export const useUpdateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/budget/${variables.year}/lines/${variables.lineId}`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const update = (year: number, budget: BudgetLine) => {
        mutate([{year, lineId: budget.id}, budget]);
    };

    return { update, ...rest };
}


export const useDeleteBudgetLine = () => {

    const queryClient = useQueryClient();

    const {mutate, ...rest} = useApiDelete<BudgetVariables>((variables) => `api/budget/${variables.year}/lines/${variables.lineId}`, {
        onSuccess: (_data, variables: BudgetVariables) => {
            let budget = queryClient.getQueryData<Budget>([budgetKey, variables.year]);
            if (!budget) return;
            budget.expensesLines = budget.expensesLines.filter(r => r.id !== (variables.lineId));
            budget.incomeLines = budget.incomeLines.filter(r => r.id !== (variables.lineId));
            queryClient.setQueryData<Budget>([budgetKey, variables.year], budget);
        }
    });

    const deleteBudgetLine = (year: number, lineId: string) => {
        mutate({year, lineId});
    }

    return { deleteBudgetLine, ...rest};
}

export const useGetTagValue = (tagId: number) =>  useApiGet<number>(["budget", "by-tag", tagId], `api/budget/tag/${tagId}`, {
    enabled: !!tagId
});
