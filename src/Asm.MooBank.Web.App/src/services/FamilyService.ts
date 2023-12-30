import { useApiGet, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import { Family } from "models";

const familyKey = "families";

export const useFamilies = (): UseQueryResult<Family[]> => useApiGet<Family[]>([familyKey], "api/families");

export const useCreateFamily = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest } = useApiPost<Family, null, Family>(() => "api/families/create", {
        onMutate: ([_variables, data]) => {
            let allFamilies = queryClient.getQueryData<Family[]>([familyKey]);
            if (!allFamilies) {
                console.warn("Query Cache is missing Families");
                return;
            }

            allFamilies.push(data);
            allFamilies = allFamilies.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Family[]>([familyKey], allFamilies);
        },
        onError: (_error, [_variables, _data]) => {
            queryClient.invalidateQueries({ queryKey: [familyKey] });
        }
    });

    const create = (family: Family) => {
        mutate([null, family]);
    };

    return { create, ...rest };
}