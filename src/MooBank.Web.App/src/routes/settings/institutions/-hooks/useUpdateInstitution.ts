import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsQueryKey, getInstitutionQueryKey, updateInstitutionMutation } from "api/@tanstack/react-query.gen";
import type { Institution } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

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
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllInstitutionsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getInstitutionQueryKey({ path: { id: variables.path.id } }) });
        }
    });

    return {
        ...rest,
        mutateAsync: (institution: Institution) =>
            toast.promise(mutateAsync({ body: institution as any, path: { id: institution.id }, query: { Name: institution.name, InstitutionType: institution.institutionType, ImporterTypeId: institution.importerTypeId } } as any),
                { pending: "Updating institution", success: "Institution updated", error: "Failed to update institution" }),
    };
}
