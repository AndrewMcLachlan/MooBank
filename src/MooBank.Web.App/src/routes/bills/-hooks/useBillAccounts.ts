import { useQuery } from "@tanstack/react-query";
import { getBillAccountsOptions } from "api/@tanstack/react-query.gen";

export const useBillAccounts = () => useQuery({
    ...getBillAccountsOptions(),
});
