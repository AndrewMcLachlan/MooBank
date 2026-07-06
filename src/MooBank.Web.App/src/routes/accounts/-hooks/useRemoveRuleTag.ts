import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstrumentRulesQueryKey } from "api/@tanstack/react-query.gen";
import { removeTagFromInstrumentRule } from "api/sdk.gen";
import type { Rule, Tag } from "api/types.gen";

export const useRemoveRuleTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (variables: { instrumentId: string, ruleId: number, tag: Tag }) =>
            removeTagFromInstrumentRule({ path: { instrumentId: variables.instrumentId, ruleId: variables.ruleId, tagId: variables.tag.id }, throwOnError: true }),
        onMutate: async (variables) => {
            const queryKey = getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } });
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<Rule[]>(queryKey);
            if (previous) {
                queryClient.setQueryData<Rule[]>(queryKey, previous.map(rule =>
                    rule.id === variables.ruleId ? { ...rule, tags: rule.tags.filter(t => t.id !== variables.tag.id) } : rule));
            }

            return { queryKey, previous };
        },
        onError: (_error, _variables, context) => {
            if (context?.previous) {
                queryClient.setQueryData(context.queryKey, context.previous);
            }
        },
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: variables.instrumentId } }) });
        },
    });
}
