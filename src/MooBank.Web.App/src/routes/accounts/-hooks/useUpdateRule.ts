import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    updateInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateRule = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateInstrumentRuleMutation(),
        onSettled: (_data, _error, variables) => {
            const accountId = variables.path?.instrumentId;
            if (!accountId) return;
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }) });
        },
    });

    return {
        ...rest,
        mutate: (variables: Parameters<typeof mutateAsync>[0]) =>
            toast.promise(mutateAsync(variables), { pending: "Updating rule", success: "Rule updated", error: "Failed to update rule" }),
    };
}
