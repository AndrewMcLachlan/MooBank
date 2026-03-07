import { useQuery } from "@tanstack/react-query";
import { getAccountsOptions } from "api/@tanstack/react-query.gen";

export const useAccounts = () => useQuery({
    ...getAccountsOptions(),
});
