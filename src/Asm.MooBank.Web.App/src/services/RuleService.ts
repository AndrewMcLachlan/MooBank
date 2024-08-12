import { useApiDelete, useApiGet, useApiPatch, useApiPost, useApiPostEmpty, useApiPutEmpty } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import * as Models from "../models";
import { Tag } from "../models";

const rulesKey = "rules";

interface RuleVariables {
    accountId: string, ruleId: number, tag: Tag,
}

export const useRules = (accountId: string): UseQueryResult<Models.Rule[]> => useApiGet<Models.Rule[]>([rulesKey, accountId], `api/accounts/${accountId}/rules`, { enabled: !!accountId });

export const useRunRules = () => useApiPostEmpty<null, { accountId: string }>((variables) => `api/accounts/${variables.accountId}/rules/run`);

export const useAddRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiPutEmpty<Models.Rule, RuleVariables>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.Rule[]>([rulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            data.tags.push(variables.tag);
            queryClient.setQueryData<Models.Rule[]>([rulesKey, variables.accountId], rules);
        },
    });
}

export const useRemoveRuleTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<RuleVariables>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const rules = queryClient.getQueryData<Models.Rule[]>([rulesKey, variables.accountId]);
            if (!rules) return;
            const data = rules.find(t => t.id === variables.ruleId);
            if (!data) return;
            const tagIndex = data.tags.findIndex(t => t.id === variables.tag.id);
            data.tags.splice(tagIndex, 1);
            queryClient.setQueryData<Models.Rule[]>([rulesKey, variables.accountId], rules);
        },
    });
}

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    return useApiPost<Models.Rule, { accountId: string }, Models.Rule>((variables) => `api/accounts/${variables.accountId}/rules`, {
        onMutate: ([variables, data]) => {
            const allRules = queryClient.getQueryData<Models.Rule[]>([rulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            const newRules  = [data, ...allRules].sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rule[]>([rulesKey, variables.accountId], newRules);
        },
        onError: (_error, [variables]) => {
            queryClient.invalidateQueries({ queryKey: [rulesKey, variables.accountId]});
        }
    });
}

export const useUpdateRule = () => {

    const queryClient = useQueryClient();

    return useApiPatch<Models.Rule, { accountId: string, id: number }, Models.Rule>((variables) => `api/accounts/${variables.accountId}/rules/${variables.id}`, {

        onMutate: ([variables, data]) => {
            let allRules = queryClient.getQueryData<Models.Rule[]>([rulesKey, variables.accountId]);
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            const ruleIndex = allRules.findIndex(r => r.id === variables.id);

            allRules.splice(ruleIndex, 1, data);

            allRules = allRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rule[]>([rulesKey, variables.accountId], allRules);
        },
        onError: (_error, [variables]) => {
            queryClient.invalidateQueries({ queryKey: [[rulesKey, variables.accountId]]});
        }
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ accountId: string; ruleId: number }>((variables) => `api/accounts/${variables.accountId}/rules/${variables.ruleId}`, {
        onSuccess: (_data, variables: { accountId: string; ruleId: number }) => {
            let allRules = queryClient.getQueryData<Models.Rule[]>([rulesKey, variables.accountId]);
            if (!allRules) return;
            allRules = allRules.filter(r => r.id !== (variables.ruleId));
            allRules = allRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.Rule[]>([rulesKey, variables.accountId], allRules);
        }
    });
}
