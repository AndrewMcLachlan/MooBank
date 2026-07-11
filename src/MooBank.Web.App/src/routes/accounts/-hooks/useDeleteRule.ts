import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    deleteInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteInstrumentRuleMutation(),
        onSettled: (_data, _error, variables) => {
            const accountId = variables.path?.instrumentId;
            if (!accountId) return;
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }) });
        },
    });

    return {
        ...rest,
        mutate: (variables: Parameters<typeof mutateAsync>[0]) =>
            toast.promise(mutateAsync(variables), { pending: "Deleting rule", success: "Rule deleted", error: "Failed to delete rule" }),
    };
}
