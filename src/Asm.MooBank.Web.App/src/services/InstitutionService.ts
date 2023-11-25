import { useApiGet } from "@andrewmclachlan/mooapp";
import { UseQueryResult } from "@tanstack/react-query";

import { Institution } from "models";

const institutionKey = "institution";

export const useInstitutions = (): UseQueryResult<Institution[]> => useApiGet<Institution[]>([institutionKey], "api/institutions", {
    //cacheTime: 1000 * 60 * 60 * 24 * 7,
    staleTime: 1000 * 60 * 60 * 24 * 7,
});