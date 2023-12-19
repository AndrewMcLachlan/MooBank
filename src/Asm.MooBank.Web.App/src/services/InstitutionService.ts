import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import { Institution } from "models";

const institutionKey = "institution";

export const useInstitutions = (): UseQueryResult<Institution[]> => useApiGet<Institution[]>([institutionKey], "api/institutions", {
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

export const useInstitution = (id: number): UseQueryResult<Institution> => useApiGet<Institution>([institutionKey, id], `api/institutions/${id}`, {
    staleTime: 1000 * 60 * 60 * 24 * 7,
});

export const useCreateInstitution = () => {

    const queryClient = useQueryClient();

    var { mutate, ...rest } = useApiPost<Institution, null, Institution>(() => "api/institutions", {
        onMutate: ([variables, data]) => {
            let allInstitutions = queryClient.getQueryData<Institution[]>([institutionKey]);
            if (!allInstitutions) {
                console.warn("Query Cache is missing Institutions");
                return;
            }

            allInstitutions.push(data);
            allInstitutions = allInstitutions.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Institution[]>([institutionKey], allInstitutions);
        },
        onError: (_error, [variables, _data]) => {
            queryClient.invalidateQueries({ queryKey: [institutionKey] });
        }
    });

    const create = (institution: Institution) => {
        mutate([null, institution]);
    };

    return { create, ...rest };
}

export const useUpdateInstitution = () => {

    const queryClient = useQueryClient();

    var { mutate, ...rest } = useApiPatch<Institution, number, Institution>((id) => `api/institutions/${id}`, {
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

    return { update, ...rest };
}