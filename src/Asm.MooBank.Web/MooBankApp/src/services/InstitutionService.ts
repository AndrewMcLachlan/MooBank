import { useApiGet } from "@andrewmclachlan/mooapp";

import { Institution } from "models";

const institutionKey = "institution";

export const useInstitutions = () => useApiGet<Institution[]>(institutionKey, "api/institutions", {
    cacheTime: 1000 * 60 * 60 * 24 * 7,
    staleTime: 1000 * 60 * 60 * 24 * 7,
});