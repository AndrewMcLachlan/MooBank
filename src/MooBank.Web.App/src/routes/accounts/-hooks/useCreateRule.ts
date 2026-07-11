import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    createInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import type { Rule } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
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

    const withToast = (variables: Parameters<typeof mutateAsync>[0]) =>
        toast.promise(mutateAsync(variables), { pending: "Creating rule", success: "Rule created", error: "Failed to create rule" });

    return {
        ...rest,
        mutate: withToast,
        mutateAsync: withToast,
    };
}
