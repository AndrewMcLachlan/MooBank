import * as Models from "../models";
import { useApiGet, useApiPost, useApiDelete, useApiDatalessPut, useApiDatalessPost, useApiPatch } from "@andrewmclachlan/mooapp";
import {  useQueryClient } from "react-query";
import { Tag } from "../models";

const rulesKey = "rules";

interface RuleVariables {
    accountId: string, ruleId: number, tag: Tag,
}

export const useRules = (accountId: string) => useApiGet<Models.Rules>([rulesKey, accountId], `api/accounts/${accountId}/rules`);

export const useRunRules = () => useApiDatalessPost<null, { accountId: string }>((variables) => `api/accounts/${variables.accountId}/rules/run`);

export const useAddRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.Rule, RuleVariables>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.Rules>([rulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            data.tags.push(variables.tag);
            queryClient.setQueryData<Models.Rules>([rulesKey, variables.accountId], rules);
        },
    });
}

export const useRemoveRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<RuleVariables>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.Rules>([rulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            const tagIndex = data.tags.findIndex(t => t.id === variables.tag.id);
            data.tags.splice(tagIndex, 1);
            queryClient.setQueryData<Models.Rules>([rulesKey, variables.accountId], rules);
        },
    });
}

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    return useApiPost<Models.Rule, { accountId: string }, Models.Rule>((variables) => `api/accounts/${variables.accountId}/rules`, {
        onMutate: ([variables, data]) => {
            const allRules = queryClient.getQueryData<Models.Rules>([rulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            allRules.rules.push(data);
            allRules.rules = allRules.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rules>([rulesKey, variables.accountId], allRules);
        },
        onError: (_error, [variables, _data]) => {
            queryClient.invalidateQueries([rulesKey, variables.accountId]);
        }
    });
}

export const useUpdateRule = () => {

    const queryClient = useQueryClient();

    return useApiPatch<Models.Rule, { accountId: string, id: number }, Models.Rule>((variables) => `api/accounts/${variables.accountId}/rules/${variables.id}`, {

        onMutate: ([variables, data]) => {
            const allRules = queryClient.getQueryData<Models.Rules>([rulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            var ruleIndex = allRules.rules.findIndex(r => r.id === variables.id);

            allRules.rules.splice(ruleIndex, 1, data);

            allRules.rules = allRules.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rules>([rulesKey, variables.accountId], allRules);
        },
        onError: (_error, [variables, _data]) => {
            queryClient.invalidateQueries([rulesKey, variables.accountId]);
        }
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ accountId: string; ruleId: number }>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}`, {
        onSuccess: (_data, variables: { accountId: string; ruleId: number }) => {
            const allRules = queryClient.getQueryData<Models.Rules>([rulesKey, variables.accountId]);
            if (!allRules) return;
            allRules.rules = allRules.rules.filter(r => r.id !== (variables.ruleId));
            allRules.rules = allRules.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rules>([rulesKey, variables.accountId], allRules);
        }
    });
}
