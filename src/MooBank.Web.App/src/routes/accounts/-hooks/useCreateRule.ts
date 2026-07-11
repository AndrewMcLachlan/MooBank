import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getAllInstrumentRulesQueryKey,
    createInstrumentRuleMutation,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createInstrumentRuleMutation(),
        onSettled: (_data, _error, variables) => {
            const accountId = variables.body?.instrumentId;
            if (!accountId) return;
            queryClient.invalidateQueries({ queryKey: getAllInstrumentRulesQueryKey({ path: { instrumentId: accountId } }) });
        },
    });

    return {
        mutateAsync: (variables: Parameters<typeof mutateAsync>[0]) =>
            toast.promise(mutateAsync(variables), { pending: "Creating rule", success: "Rule created", error: "Failed to create rule" }),
        ...rest,
    };
}
