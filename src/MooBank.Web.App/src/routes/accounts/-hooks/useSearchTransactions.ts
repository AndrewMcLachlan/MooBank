import { useQuery } from "@tanstack/react-query";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import type { Transaction, TransactionType } from "api/types.gen";
import {
    searchTransactionsOptions,
} from "api/@tanstack/react-query.gen";

export const useSearchTransactions = (transaction: Transaction, searchType: TransactionType) => {

    return useQuery({
        ...searchTransactionsOptions({
            path: { instrumentId: transaction.accountId },
            query: {
                Start: format(parseISO(transaction.transactionTime), 'yyyy-MM-dd'),
                TransactionType: searchType,
                TagIds: transaction.tags.map(t => t.id),
            },
        }),
    });
}
