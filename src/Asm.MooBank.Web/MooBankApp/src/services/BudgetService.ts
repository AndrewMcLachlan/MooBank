import { useQueryClient } from "react-query";
import { Budget, BudgetLine } from "models";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

interface BudgetVariables {
    accountId: string,
    year: number,
    lineId?: string,
}

export const budgetKey = "budget";

export const useBudgetYears = (accountId: string) => useApiGet<number[]>([budgetKey, accountId], `api/accounts/${accountId}/budget`);

export const useBudget = (accountId: string, year: number) => useApiGet<Budget>([budgetKey, accountId, year], `api/accounts/${accountId}/budget/${year}`);

export const useCreateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/accounts/${variables.accountId}/budget/${variables.year}/lines`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const create = (accountId: string, year: number, budgetLine: BudgetLine) => {
        mutate([{accountId, year}, budgetLine]);
    };

    return { create, ...rest };
}

export const useUpdateBudgetLine = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/accounts/${variables.accountId}/budget/${variables.year}/lines/${variables.lineId}`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const update = (accountId: string, year: number, budget: BudgetLine) => {
        mutate([{accountId, year, lineId: budget.id}, budget]);
    };

    return { update, ...rest };
}


export const useDeleteBudgetLine = () => {

    const queryClient = useQueryClient();

    const {mutate, ...rest} = useApiDelete<BudgetVariables>((variables) => `api/accounts/${variables.accountId}/budget/${variables.year}/lines/${variables.lineId}`, {
        onSuccess: (_data, variables: BudgetVariables) => {
            let budget = queryClient.getQueryData<Budget>([budgetKey, variables.accountId, variables.year]);
            if (!budget) return;
            budget.expensesLines = budget.expensesLines.filter(r => r.id !== (variables.lineId));
            budget.incomeLines = budget.incomeLines.filter(r => r.id !== (variables.lineId));
            queryClient.setQueryData<Budget>([budgetKey, variables.accountId, variables.year], budget);
        }
    });

    const deleteBudgetLine = (accountId: string, year: number, lineId: string) => {
        mutate({accountId, year, lineId});
    }

    return { deleteBudgetLine, ...rest};
}

export const useGetTagValue = (accountId: string, tagId: number) =>  useApiGet<number>(["budget", accountId, "by-tag", tagId], `api/accounts/${accountId}/budget/tag/${tagId}`, {
    enabled: !!accountId && !!tagId
});
