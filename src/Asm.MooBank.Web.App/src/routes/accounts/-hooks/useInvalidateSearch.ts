import { useQueryClient } from "@tanstack/react-query";
import {
    searchTransactionsOptions,
} from "api/@tanstack/react-query.gen";

export const useInvalidateSearch = (transactionId: string) => {

    const queryClient = useQueryClient();

    return () => queryClient.invalidateQueries({ queryKey: searchTransactionsOptions({ path: { instrumentId: transactionId } } as any).queryKey });
}
