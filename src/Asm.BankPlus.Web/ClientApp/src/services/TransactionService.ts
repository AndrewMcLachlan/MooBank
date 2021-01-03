import { useQueryClient } from "react-query";

import * as Models from "../models";
import { TransactionTag } from "../models";
import { useApiGet, useApiDelete, useApiDatalessPut } from "./api";

const transactionKey = "transactions";

interface TransactionTagVariables {
    transactionId: string,
    tag: TransactionTag,
}

export const useTransactions = (accountId: string, filterTagged: boolean, pageSize: number, pageNumber: number) =>
    useApiGet<Models.Transactions>([transactionKey], `api/accounts/${accountId}/transactions/${filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}`);

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.Transaction, TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            const transactions = queryClient.getQueryData<Models.Transactions>([transactionKey]);
            const data = transactions.transactions.find(t => t.id === variables.transactionId);
            data.tags.push(variables.tag);
            queryClient.setQueryData<Models.Transactions>([transactionKey], transactions);
        },
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onSuccess: (_data: null, variables) => {
            const transaction = queryClient.getQueryData<Models.Transaction>([transactionKey, { id: variables.transactionId }]);
            transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);
            queryClient.setQueryData<Models.Transaction>([transactionKey, { id: variables.transactionId }], transaction);
        }
    });
}
