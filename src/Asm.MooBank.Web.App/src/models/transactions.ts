import { emptyGuid } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import type { TransactionSplit } from "api/types.gen";

export interface TransactionUpdate {
    excludeFromReporting: boolean;
    notes?: string;
    splits?: TransactionSplit[];
}

export const emptyTransactionSplit: TransactionSplit = {
    id: emptyGuid,
    amount: 0,
    offsetBy: [],
    tags: [],
};

export interface CreateTransaction {
    amount: number;
    description: string;
    reference?: string;
    transactionTime: string;
}

export const emptyTransaction: CreateTransaction = {
    amount: 0,
    description: "",
    reference: "",
    transactionTime: format(new Date(), 'yyyy-MM-dd'),
};
