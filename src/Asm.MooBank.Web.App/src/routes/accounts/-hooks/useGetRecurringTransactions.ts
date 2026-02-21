import { useQuery } from "@tanstack/react-query";
import {
    getRecurringTransactionsForAVirtualAccountOptions,
} from "api/@tanstack/react-query.gen";

export const useGetRecurringTransactions = (accountId: string, virtualAccountId: string) =>
    useQuery({
        ...getRecurringTransactionsForAVirtualAccountOptions({ path: { accountId, virtualAccountId } }),
    });
