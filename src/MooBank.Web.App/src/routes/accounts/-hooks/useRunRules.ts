import { useMutation } from "@tanstack/react-query";
import { runRulesMutation } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useRunRules = () => {

    // Running rules is queued (202 Accepted) and processed by a background job, so the toast
    // reports queuing, not completion, and the transaction list is not invalidated here — the
    // retagging happens later, out of band, with no completion signal to react to.
    const { mutateAsync, ...rest } = useMutation({
        ...runRulesMutation(),
    });

    return {
        ...rest,
        mutate: (variables: { path: { instrumentId: string } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Running rules", success: "Rule run started", error: "Failed to run rules" }),
    };
};
