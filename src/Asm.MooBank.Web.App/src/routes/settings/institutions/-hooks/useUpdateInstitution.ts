import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsQueryKey, getInstitutionQueryKey, updateInstitutionMutation } from "api/@tanstack/react-query.gen";
import type { Institution } from "api/types.gen";

export const useUpdateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateInstitutionMutation(),
        onMutate: (variables) => {
            let allInstitutions = queryClient.getQueryData<Institution[]>(getAllInstitutionsQueryKey());
            if (!allInstitutions) {
                console.warn("Query Cache is missing Institutions");
                return;
            }

            allInstitutions = allInstitutions.filter(t => t.id !== variables.path.id);
            allInstitutions.push(variables.body as Institution);

            allInstitutions = allInstitutions.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Institution[]>(getAllInstitutionsQueryKey(), allInstitutions);
        },
        onError: (_error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllInstitutionsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getInstitutionQueryKey({ path: { id: variables.path.id } }) });
        }
    });

    return {
        mutateAsync: (institution: Institution) =>
            mutateAsync({ body: institution as any, path: { id: institution.id }, query: { Name: institution.name, InstitutionType: institution.institutionType } } as any),
        ...rest,
    };
}
