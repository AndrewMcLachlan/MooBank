import { useQueryClient } from "react-query";
import { BudgetLine } from "models";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { ByTagReport, TagValue } from "models/reports";

interface BudgetVariables {
    accountId: string,
    id?: string,
}

export const budgetKey = "budget";

export const useBudget = (accountId: string) => useApiGet<BudgetLine[]>([budgetKey, accountId], `api/accounts/${accountId}/budget`);

export const useCreateBudget = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPost<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/accounts/${variables.accountId}/budget`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const create = (accountId: string, budgetLine: BudgetLine) => {
        mutate([{accountId}, budgetLine]);
    };

    return { create, ...rest };
}

export const useUpdateBudget = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest} = useApiPatch<BudgetLine, BudgetVariables, BudgetLine>((variables) => `api/accounts/${variables.accountId}/budget/${variables.id}`, {
        onSettled: () => {
            queryClient.invalidateQueries(budgetKey);
        }
    });

    const update = (accountId: string, budget: BudgetLine) => {
        mutate([{accountId, id: budget.id}, budget]);
    };

    return { update, ...rest };
}


export const useDeleteBudgetLine = () => {

    const queryClient = useQueryClient();

    return useApiDelete<BudgetVariables>((variables) => `api/accounts/${variables.accountId}/budget/${variables.id}`, {
        onSuccess: (_data, variables: BudgetVariables) => {
            let budgetLines = queryClient.getQueryData<BudgetLine[]>([budgetKey, variables.accountId]);
            if (!budgetLines) return;
            budgetLines = budgetLines.filter(r => r.id !== (variables.id));
            budgetLines = budgetLines.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<BudgetLine[]>([budgetKey, variables.accountId], budgetLines);
        }
    });
}

export const useGetTagValue = (accountId: string, tagId: number) =>  useApiGet<ByTagReport>(["budget", accountId, "by-tag", tagId], `api/accounts/${accountId}/budget/tag/${tagId}`, {
    enabled: !!accountId && !!tagId
});
