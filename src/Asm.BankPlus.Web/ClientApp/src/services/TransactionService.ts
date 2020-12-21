import { AxiosResponse } from "axios";
import { useMutation, useQueryClient } from "react-query";

import * as Models from "../models";
import { httpClient } from "./HttpClient";
import { useApiQuery } from "./useApiQuery";

export const useTransactions = (accountId: string, filterTagged: boolean, pageSize: number, pageNumber: number) =>
    useApiQuery<Models.Transactions>(["transactions", accountId, filterTagged, pageSize, pageNumber], `api/accounts/${accountId}/transactions/${filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}`);

interface TransactionTagVariables {
    transactionId: string,
    tagId: number,
}

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.Transaction, null, TransactionTagVariables>(async (variables) => (await httpClient.put<Models.Transaction>(`api/transactions/${variables.transactionId}/tag/${variables.tagId}`)).data, {
        onSuccess: (data: Models.Transaction, variables: TransactionTagVariables) => {
            queryClient.setQueryData<Models.Transaction>(["transactions", { id: variables.transactionId }], data);
        }
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.Transaction, null, TransactionTagVariables>(async (variables) => (await httpClient.delete<Models.Transaction>(`api/transactions/${variables.transactionId}/tag/${variables.tagId}`)).data, {
        onSuccess: (data: Models.Transaction, variables: TransactionTagVariables) => {
            queryClient.setQueryData<Models.Transaction>(["transactions", { id: variables.transactionId }], data);
        }
    });
}
