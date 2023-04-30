import { TransactionTag } from "./TransactionTag";

export enum TransactionType {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

export interface Transaction {
    id: string;
    reference: string;
    accountId: string;
    amount: number;
    description: string;
    transactionTime: string;
    transactionType: TransactionType;
    tags: TransactionTag[];
    notes?: string;
    extraInfo: any;
}

export interface TransactionUpdate {
    notes?: string;
}