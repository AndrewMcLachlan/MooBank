import { useQueryClient } from "@tanstack/react-query";
import {
    searchTransactionsQueryKey,
} from "api/@tanstack/react-query.gen";

export const useInvalidateSearch = (accountId: string) => {

    const queryClient = useQueryClient();

    return () => queryClient.invalidateQueries({ queryKey: searchTransactionsQueryKey({ path: { instrumentId: accountId } } as any) });
}
