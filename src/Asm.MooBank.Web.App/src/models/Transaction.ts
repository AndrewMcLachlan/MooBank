import { Tag } from "./Tag";
import { emptyGuid } from "@andrewmclachlan/mooapp";
import { format } from "date-fns/format";

enum TransactionTypesEnum {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

//export const TransactionSubTypes = ["Credit", "Debit", "RecurringCredit", "RecurringDebit", "BalanceAdjustment"] as TransactionType[];
//export type TransactionSubType = "Credit" | "Debit" | "RecurringCredit" | "RecurringDebit" | "BalanceAdjustment";

export const TransactionTypes = ["Credit", "Debit"] as TransactionType[];
export type TransactionType = "Credit" | "Debit";


export const isCredit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType] % 2 === 1;

export const isDebit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType] % 2 === 0;

export interface Transaction {
    id: string;
    accountId: string;
    amount: number;
    description: string;
    location?: string;
    reference?: string;
    accountHolderName?: string;
    purchaseDate?: string;
    transactionTime: string;
    transactionType: TransactionType;
    tags: Tag[];
    notes?: string;
    excludeFromReporting?: boolean;
    splits: TransactionSplit[];
    offsetFor: TransactionOffset[];
    extraInfo: any;
}

export interface TransactionUpdate {
    excludeFromReporting: boolean;
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

export interface CreateTransaction {
    //id: string;
    amount: number;
    description: string;
    reference?: string;
    transactionTime: string;
}

export const emptyTransaction: CreateTransaction = {
    //id: emptyGuid,
    amount: 0,
    description: "",
    reference: "",
    transactionTime: format(new Date(), 'yyyy-MM-dd'),
}
