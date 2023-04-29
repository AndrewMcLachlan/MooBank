import * as Models from "models";
import { useApiGet, useApiPost, useApiDelete, useApiDatalessPut, useApiDatalessPost, useApiPatch, useHttpClient } from "@andrewmclachlan/mooapp";
import { UseMutationOptions, useMutation, useQueryClient } from "react-query";
import { TransactionTag } from "models";
import { REFUSED } from "dns";
import { AxiosResponse } from "axios";

const transactionRulesKey = "transactionrules";

interface TransactionTagRuleVariables {
    accountId: string, ruleId: number, tag: TransactionTag,
}

export const useRules = (accountId: string) => useApiGet<Models.TransactionTagRules>([transactionRulesKey, accountId], `api/accounts/${accountId}/transaction/tag/rules`);

export const useRunRules = () => useApiDatalessPost<null, { accountId: string }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/run`);

export const useAddTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.TransactionTagRule, TransactionTagRuleVariables>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            data.tags.push(variables.tag);
            queryClient.setQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId], rules);
        },
    });
}

export const useRemoveTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<TransactionTagRuleVariables>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            const tagIndex = data.tags.findIndex(t => t.id === variables.tag.id);
            data.tags.splice(tagIndex, 1);
            queryClient.setQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId], rules);
        },
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

export const useUpdateRule = () => {

    const queryClient = useQueryClient();

    return useApiPatch<Models.TransactionTagRule, { accountId: string, id: number }, Models.TransactionTagRule>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.id}`, {

        onMutate: ([variables, data]) => {
            const allRules = queryClient.getQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            var ruleIndex = allRules.rules.findIndex(r => r.id === variables.id);

            allRules.rules.splice(ruleIndex, 1, data);

            allRules.rules = allRules.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRules>([transactionRulesKey, variables.accountId], allRules);
        },
        onError: (_error, [variables, _data]) => {
            queryClient.invalidateQueries([transactionRulesKey, variables.accountId]);
        }
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ accountId: string; ruleId: number }>((variables) => `api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}`, {
        onSuccess: (_data, variables: { accountId: string; ruleId: number }) => {
            let allTags = queryClient.getQueryData<Models.TransactionTagRule[]>([transactionRulesKey]);
            if (!allTags) return;
            allTags = allTags.filter(r => r.id !== (variables.ruleId));
            allTags = allTags.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRule[]>([transactionRulesKey], allTags);
        }
    });
}
