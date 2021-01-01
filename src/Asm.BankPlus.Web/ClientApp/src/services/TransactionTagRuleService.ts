import * as Models from "../models";
import { useApiGet, useApiPost, useApiPut, useApiDelete, useApiDatalessPut, useApiDatalessPost } from "./api";
import { useQueryClient } from "react-query";

const transactionRulesKey = "transactionrules";

export const useRules = (accountId: string) => useApiGet<Models.TransactionTagRules>([transactionRulesKey, accountId], `api/accounts/${accountId}/transaction/tag/rules`);

export const useRunRules = () => useApiDatalessPost<null, { accountId: string }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/run`);

export const useAddTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.TransactionTagRule, { accountId: string, ruleId: number, tagId: number }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tagId}`, {
        onSuccess: (data: Models.TransactionTagRule, variables: { accountId: string, ruleId: number, tagId: number }) => {
            queryClient.setQueryData<Models.TransactionTagRule>([transactionRulesKey, variables.accountId, { id: variables.ruleId }], data);
        }
    });
}

export const useRemoveTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ accountId: string, ruleId: number, tagId: number }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tagId}`, {
        onSuccess: (data: Models.TransactionTagRule, variables: { accountId: string, ruleId: number, tagId: number }) => {
            queryClient.setQueryData<Models.TransactionTagRule>([transactionRulesKey, variables.accountId, { id: variables.ruleId }], data);
        }
    });
}

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    return useApiPost<Models.TransactionTagRule, { accountId: string }, Models.TransactionTagRule>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules`, {
        onMutate: ([variables, data]) => {
            const allRules = queryClient.getQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }
            allRules.rules.push(data);
            allRules.rules = allRules.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId], allRules);
        },
        onSuccess: () => {
            //queryClient.invalidateQueries([transactionRulesKey]);
        }
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ accountId: string; ruleId: number }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}`, {
        onSuccess: (variables: { accountId: string; ruleId: number }) => {
            let allTags = queryClient.getQueryData<Models.TransactionTagRule[]>([transactionRulesKey]);
            allTags = allTags.filter(r => r.id !== (variables.ruleId));
            allTags = allTags.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRule[]>([transactionRulesKey], allTags);
        }
    });
}
