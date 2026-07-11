import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsQueryKey, getInstitutionQueryKey, updateInstitutionMutation } from "api/@tanstack/react-query.gen";
import type { Institution } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateInstitutionMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllInstitutionsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getInstitutionQueryKey({ path: { id: variables.path.id } }) });
        }
    });

    return {
        mutateAsync: (institution: Institution) =>
            toast.promise(mutateAsync({ body: institution as any, path: { id: institution.id }, query: { Name: institution.name, InstitutionType: institution.institutionType, ImporterTypeId: institution.importerTypeId } } as any), { pending: "Updating institution", success: "Institution updated", error: "Failed to update institution" }),
        ...rest,
    };
}
