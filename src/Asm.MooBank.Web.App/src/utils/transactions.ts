import type { TransactionType, TransactionSplit } from "api/types.gen";

enum TransactionTypesEnum {
    Credit = 1,
    Debit = 2,
    RecurringCredit = 3,
    RecurringDebit = 4,
    BalanceAdjustment = 5,
}

export const isCredit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType as keyof typeof TransactionTypesEnum] % 2 === 1;

export const isDebit = (transactionType: TransactionType) => TransactionTypesEnum[transactionType as keyof typeof TransactionTypesEnum] % 2 === 0;

export const getSplitTotal = (splits: TransactionSplit[]) => splits.reduce((total, split) => total + Number(split.amount), 0);
