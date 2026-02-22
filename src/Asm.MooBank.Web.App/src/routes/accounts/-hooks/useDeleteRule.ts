import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    deleteInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import type { Rule } from "api/types.gen";

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
