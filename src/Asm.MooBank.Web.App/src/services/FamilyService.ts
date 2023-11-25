import { useApiGet } from "@andrewmclachlan/mooapp";
import { UseQueryResult } from "@tanstack/react-query";

import { Family } from "models";

const familyKey = "families";

export const useFamilies = (): UseQueryResult<Family[]> => useApiGet<Family[]>([familyKey], "api/families");
