import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    addTagToInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import type { Rule, Tag } from "api/types.gen";

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
