import { emptyGuid } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import type { TransactionType, TransactionSplit } from "api/types.gen";

enum TransactionTypesEnum {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

export const TransactionTypes: TransactionType[] = ["Credit", "Debit"];

export const isCredit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType as keyof typeof TransactionTypesEnum] % 2 === 1;

export const isDebit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType as keyof typeof TransactionTypesEnum] % 2 === 0;

export interface TransactionUpdate {
    excludeFromReporting: boolean;
    notes?: string;
    splits?: TransactionSplit[];
}

export interface TransactionOffsetUpdate {
    transactionOffsetId: string;
    amount: number;
}

export const emptyTransactionSplit: TransactionSplit = {
    id: emptyGuid,
    amount: 0,
    offsetBy: [],
    tags: [],
};

export const getSplitTotal = (splits: TransactionSplit[]) => splits.reduce((total, split) => total + Number(split.amount), 0);

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
