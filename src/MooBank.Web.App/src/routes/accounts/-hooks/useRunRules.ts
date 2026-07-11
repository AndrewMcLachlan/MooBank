import { useMutation, useQueryClient } from "@tanstack/react-query";
import { runRulesMutation } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { invalidateTransactionLists } from "./transactionKeys";

export const useRunRules = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...runRulesMutation(),
        onSettled: () => invalidateTransactionLists(queryClient),
    });

    return {
        ...rest,
        mutate: (variables: { path: { instrumentId: string } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Running rules", success: "Rules run", error: "Failed to run rules" }),
    };
};
