import { useQuery } from "@tanstack/react-query";
import { getInstitutionOptions } from "api/@tanstack/react-query.gen";

export const useInstitution = (id: number) => useQuery({
    ...getInstitutionOptions({ path: { id } }),
    staleTime: 1000 * 60 * 60 * 24 * 7,
});
