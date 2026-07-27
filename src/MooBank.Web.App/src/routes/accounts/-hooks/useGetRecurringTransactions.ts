import { useQuery } from "@tanstack/react-query";
import {
    getRecurringTransactionsOptions,
} from "api/@tanstack/react-query.gen";

export const useGetRecurringTransactions = (instrumentId: string, virtualInstrumentId: string) =>
    useQuery({
        ...getRecurringTransactionsOptions({ path: { instrumentId, virtualInstrumentId } }),
    });
