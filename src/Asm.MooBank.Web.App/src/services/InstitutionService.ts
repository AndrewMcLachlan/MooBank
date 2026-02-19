import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getAllInstitutionsOptions, getAllInstitutionsQueryKey, getInstitutionOptions, getInstitutionQueryKey, createInstitutionMutation, updateInstitutionMutation } from "api/@tanstack/react-query.gen";

import type { AccountType, Institution } from "api/types.gen";

export const useInstitutions = () => useQuery({
    ...getAllInstitutionsOptions(),
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

export const useInstitutionsByAccountType = (accountType?: AccountType) => useQuery({
    ...getAllInstitutionsOptions({ query: { AccountType: accountType as Exclude<AccountType, "None"> } }),
    staleTime: 1000 * 60 * 60 * 24 * 7,
    enabled: (accountType as string) !== "None" && accountType !== undefined,
});

export const useInstitution = (id: number) => useQuery({
    ...getInstitutionOptions({ path: { id } }),
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

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
