import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesOptions,
    getAllInstrumentRulesQueryKey,
    runRulesMutation,
    addTagToInstrumentRuleMutation,
    removeTagFromInstrumentRuleMutation,
    createInstrumentRuleMutation,
    updateInstrumentRuleMutation,
    deleteInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import type { Rule, Tag } from "api/types.gen";

export const useRules = (accountId: string) => useQuery({
    ...getAllInstrumentRulesOptions({ path: { instrumentId: accountId } }),
    enabled: !!accountId,
});

export const useRunRules = () => useMutation({ ...runRulesMutation() });

export const useAddRuleTag = () => {

    const queryClient = useQueryClient();

    const { mutate: rawMutate, ...rest } = useMutation({ ...addTagToInstrumentRuleMutation() });

    const mutate = (variables: { instrumentId: string, ruleId: number, tag: Tag }) => {
        const rules = queryClient.getQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } }));
        if (rules) {
            const data = rules.find(t => t.id === variables.ruleId);
            if (data) {
                data.tags.push(variables.tag);
                queryClient.setQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } }), rules);
            }
        }
        rawMutate({ path: { instrumentId: variables.instrumentId, ruleId: variables.ruleId, tagId: variables.tag.id } });
    };

    return { mutate, ...rest };
}

export const useRemoveRuleTag = () => {

    const queryClient = useQueryClient();

    const { mutate: rawMutate, ...rest } = useMutation({ ...removeTagFromInstrumentRuleMutation() });

    const mutate = (variables: { instrumentId: string, ruleId: number, tag: Tag }) => {
        const rules = queryClient.getQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } }));
        if (rules) {
            const data = rules.find(t => t.id === variables.ruleId);
            if (data) {
                const tagIndex = data.tags.findIndex(t => t.id === variables.tag.id);
                data.tags.splice(tagIndex, 1);
                queryClient.setQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } }), rules);
            }
        }
        rawMutate({ path: { instrumentId: variables.instrumentId, ruleId: variables.ruleId, tagId: variables.tag.id } });
    };

    return { mutate, ...rest };
}

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...createInstrumentRuleMutation(),
        onMutate: (variables) => {
            const accountId = variables.body?.instrumentId;
            if (!accountId) return;
            const allRules = queryClient.getQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }));
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            const newRules = [variables.body as unknown as Rule, ...allRules].sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }), newRules);
        },
        onSettled: (_data, _error, variables) => {
            const accountId = variables.body?.instrumentId;
            if (!accountId) return;
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }) });
        },
    });
}

export const useUpdateRule = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...updateInstrumentRuleMutation(),
        onMutate: (variables) => {
            const accountId = variables.path?.instrumentId;
            if (!accountId) return;
            let allRules = queryClient.getQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }));
            if (!allRules) {
                console.warn("Query Cache is missing Transaction Rules");
                return;
            }

            const ruleIndex = allRules.findIndex(r => r.id === variables.path?.ruleId);

            allRules.splice(ruleIndex, 1, variables.body as unknown as Rule);

            allRules = allRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }), allRules);
        },
        onSettled: (_data, _error, variables) => {
            const accountId = variables.path?.instrumentId;
            if (!accountId) return;
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }) });
        },
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...deleteInstrumentRuleMutation(),
        onSuccess: (_data, variables) => {
            const accountId = variables.path?.instrumentId;
            const ruleId = variables.path?.ruleId;
            if (!accountId) return;
            let allRules = queryClient.getQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }));
            if (!allRules) return;
            allRules = allRules.filter(r => r.id !== ruleId);
            allRules = allRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Rule[]>(getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }), allRules);
        },
    });
}
