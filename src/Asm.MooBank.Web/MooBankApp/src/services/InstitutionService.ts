import { useApiGet } from "@andrewmclachlan/mooapp";

import { Institution } from "models";

const institutionKey = "institution";

export const useInstitutions = () => useApiGet<Institution[]>(institutionKey, "api/institutions");