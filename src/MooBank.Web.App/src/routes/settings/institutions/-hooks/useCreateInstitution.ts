import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsQueryKey, createInstitutionMutation } from "api/@tanstack/react-query.gen";
import type { Institution } from "api/types.gen";

export const useCreateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createInstitutionMutation(),
        onMutate: (variables) => {
            let allInstitutions = queryClient.getQueryData<Institution[]>(getAllInstitutionsQueryKey());
            if (!allInstitutions) {
                console.warn("Query Cache is missing Institutions");
                return;
            }

            allInstitutions.push(variables.body as Institution);
            allInstitutions = allInstitutions.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Institution[]>(getAllInstitutionsQueryKey(), allInstitutions);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: getAllInstitutionsQueryKey() });
        }
    });

    return {
        mutateAsync: (institution: Institution) =>
            mutateAsync({ body: institution as any }),
        ...rest,
    };
}
