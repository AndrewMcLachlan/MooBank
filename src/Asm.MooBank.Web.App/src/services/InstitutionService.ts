import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import { AccountType, Institution } from "models";

const institutionKey = "institution";

export const useInstitutions = (): UseQueryResult<Institution[]> => useApiGet<Institution[]>([institutionKey], "api/institutions", {
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

export const useInstitutionsByAccountType = (accountType?: AccountType): UseQueryResult<Institution[]> => useApiGet<Institution[]>([institutionKey, accountType], `api/institutions?accountType=${accountType}`, {
    staleTime: 1000 * 60 * 60 * 24 * 7,
    enabled: accountType !== "None" && accountType !== undefined,
});

export const useInstitution = (id: number): UseQueryResult<Institution> => useApiGet<Institution>([institutionKey, id], `api/institutions/${id}`, {
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

export const useCreateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<Institution, null, Institution>(() => "api/institutions", {
        onMutate: ([_variables, data]) => {
            let allInstitutions = queryClient.getQueryData<Institution[]>([institutionKey]);
            if (!allInstitutions) {
                console.warn("Query Cache is missing Institutions");
                return;
            }

            allInstitutions.push(data);
            allInstitutions = allInstitutions.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Institution[]>([institutionKey], allInstitutions);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: [institutionKey] });
        }
    });

    const create = (institution: Institution) => {
        mutate([null, institution]);
    };

    return create;
}

export const useUpdateInstitution = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<Institution, number, Institution>((id) => `api/institutions/${id}`, {
        onMutate: ([id, data]) => {
            let allInstitutions = queryClient.getQueryData<Institution[]>([institutionKey]);
            if (!allInstitutions) {
                console.warn("Query Cache is missing Institutions");
                return;
            }

            allInstitutions = allInstitutions.filter(t => t.id !== id);
            allInstitutions.push(data);

            allInstitutions = allInstitutions.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Institution[]>([institutionKey], allInstitutions);
        },
        onError: (_error, [id, _data]) => {
            queryClient.invalidateQueries({ queryKey: [institutionKey] });
            queryClient.invalidateQueries({ queryKey: [institutionKey, id] });
        }
    });

    const update = (institution: Institution) => {
        mutate([null, institution]);
    };

    return update;
}
