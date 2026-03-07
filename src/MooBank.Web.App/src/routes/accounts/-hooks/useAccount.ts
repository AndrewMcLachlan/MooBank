import { useQuery } from "@tanstack/react-query";
import { getAccountOptions } from "api/@tanstack/react-query.gen";

export const useAccount = (accountId: string) => useQuery({
    ...getAccountOptions({ path: { instrumentId: accountId } }),
});
