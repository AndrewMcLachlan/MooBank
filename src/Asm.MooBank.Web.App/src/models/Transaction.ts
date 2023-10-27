import { v4 as guid } from "uuid";

import { Tag } from "./Tag";
import { emptyGuid } from "@andrewmclachlan/mooapp";

enum TransactionTypesEnum {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

export const TransactionTypes = ["Credit", "Debit", "RecurringCredit", "RecurringDebit", "BalanceAdjustment"] as TransactionType[];
export type TransactionType = "Credit" | "Debit" | "RecurringCredit" | "RecurringDebit" | "BalanceAdjustment";


export const isCredit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType] % 2 === 1;

export const isDebit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType] % 2 === 0;

export interface Transaction {
    id: string;
    accountId: string;
    amount: number;
    netAmount: number;
    description: string;
    location?: string;
    reference?: string;
    accountHolderName?: string;
    purchaseDate?: string;
    transactionTime: string;
    transactionType: TransactionType;
    tags: Tag[];
    notes?: string;
    splits: TransactionSplit[];
    offsetFor: TransactionOffset[];
    extraInfo: any;
}

export interface TransactionUpdate {
    notes?: string;
    splits?: TransactionSplit[];
}

export interface TransactionOffsetUpdate {
    transactionOffsetId: string;
    amount: number;
}

export interface TransactionOffset {
    transaction: Transaction;
    amount: number;
}

export interface TransactionSplit {
    id: string;
    amount: number;
    offsetBy: TransactionOffset[];
    tags: Tag[];
}

export const emptyTransactionSplit: TransactionSplit = {
    id: emptyGuid,
    amount: 0,
    offsetBy: [],
    tags: [],
};

export const getSplitTotal = (splits: TransactionSplit[]) => splits.reduce((total, split) => total + split.amount, 0);
