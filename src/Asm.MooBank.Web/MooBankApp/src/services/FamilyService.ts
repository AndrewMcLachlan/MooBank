import { useApiGet } from "@andrewmclachlan/mooapp";

import { Family } from "models";

const familyKey = "families";

export const useFamilies = () => useApiGet<Family[]>(familyKey, "api/families");
