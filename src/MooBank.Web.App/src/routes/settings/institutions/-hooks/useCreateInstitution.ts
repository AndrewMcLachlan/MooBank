import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsQueryKey, createInstitutionMutation } from "api/@tanstack/react-query.gen";
import type { Institution } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createInstitutionMutation(),
        onMutate: async (variables) => {
            const queryKey = getAllInstitutionsQueryKey();
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<Institution[]>(queryKey);
            if (previous) {
                const next = [...previous, variables.body as Institution].sort((i1, i2) => i1.name.localeCompare(i2.name));
                queryClient.setQueryData<Institution[]>(queryKey, next);
            }

            return { previous };
        },
        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(getAllInstitutionsQueryKey(), context.previous);
            }
        },
        onSettled: () => {
            // Refetch so the optimistic entry (which has no server-assigned id) is replaced.
            queryClient.invalidateQueries({ queryKey: getAllInstitutionsQueryKey() });
        },
    });

    return {
        mutateAsync: (institution: Institution) =>
            toast.promise(mutateAsync({ body: institution as any }), { pending: "Creating institution", success: "Institution created", error: "Failed to create institution" }),
        ...rest,
    };
}
