import { useQueryClient } from "react-query";

import * as Models from "../models";
import { useApiGet, useApiPut, useApiDelete, useApiDatalessPut } from "./api";

const transactionKey = "transactions";

interface TransactionTagVariables {
    transactionId: string,
    tagId: number,
}

export const useTransactions = (accountId: string, filterTagged: boolean, pageSize: number, pageNumber: number) =>
    useApiGet<Models.Transactions>([transactionKey, accountId, filterTagged, pageSize, pageNumber], `api/accounts/${accountId}/transactions/${filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}`);

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.Transaction, TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tagId}`, {
        onSuccess: (data: Models.Transaction, variables) => {
            queryClient.setQueryData<Models.Transaction>([transactionKey, { id: variables.transactionId }], data);
        }
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tagId}`, {
        onSuccess: (_data: null, variables) => {
            const transaction = queryClient.getQueryData<Models.Transaction>([transactionKey, { id: variables.transactionId }]);
            transaction.tags = transaction.tags.filter(t => t.id !== variables.tagId);
            queryClient.setQueryData<Models.Transaction>([transactionKey, { id: variables.transactionId }], transaction);
        }
    });
}
