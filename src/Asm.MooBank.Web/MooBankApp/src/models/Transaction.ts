import { Tag } from "./Tag";

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
    tags: Tag[];
    notes?: string;
    offsetBy?: TransactionOffset[];
    offsets: TransactionOffset[];
    extraInfo: any;
}

export interface TransactionUpdate {
    notes?: string;
    offsetBy?: TransactionOffsetUpdate[];
}

export interface TransactionOffsetUpdate {
    transactionOffsetId: string;
    amount: number;
}

export interface TransactionOffset {
    transaction: Transaction;
    amount: number;
}