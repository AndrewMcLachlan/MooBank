import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    updateInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import type { Rule } from "api/types.gen";

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
