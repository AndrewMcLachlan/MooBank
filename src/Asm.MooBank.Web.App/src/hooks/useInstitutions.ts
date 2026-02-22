import { useQuery } from "@tanstack/react-query";
import { getAllInstitutionsOptions } from "api/@tanstack/react-query.gen";

export const useInstitutions = () => useQuery({
    ...getAllInstitutionsOptions(),
    staleTime: 1000 * 60 * 60 * 24 * 7,
});
