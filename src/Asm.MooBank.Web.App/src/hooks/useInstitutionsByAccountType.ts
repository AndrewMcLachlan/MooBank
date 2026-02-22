import { useQuery } from "@tanstack/react-query";
import { getAllInstitutionsOptions } from "api/@tanstack/react-query.gen";
import type { AccountType } from "api/types.gen";

export const useInstitutionsByAccountType = (accountType?: AccountType) => useQuery({
    ...getAllInstitutionsOptions({ query: { AccountType: accountType as Exclude<AccountType, "None"> } }),
    staleTime: 1000 * 60 * 60 * 24 * 7,
    enabled: (accountType as string) !== "None" && accountType !== undefined,
});
