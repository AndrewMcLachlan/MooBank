import { TransactionTag } from "./TransactionTag";

export enum TransactionType {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

export const isCredit = (transactionType: TransactionType) => transactionType % 2 === 1;

export const isDebit = (transactionType: TransactionType) => transactionType % 2 === 0;

export interface Transaction {
    id: string;
    reference: string;
    accountId: string;
    amount: number;
    netAmount: number;
    description: string;
    transactionTime: string;
    transactionType: TransactionType;
    tags: TransactionTag[];
    notes?: string;
    offsetBy?: Transaction;
    offsets: Transaction;
    extraInfo: any;
}

export interface TransactionUpdate {
    notes?: string;
    offsetByTransactionId?: string;
}