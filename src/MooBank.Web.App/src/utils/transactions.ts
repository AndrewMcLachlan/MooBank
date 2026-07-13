import type { TransactionType, TransactionSplit } from "api/types.gen";

// TransactionType is NotSet | Credit | Debit (see MooBank.Primitives/TransactionType.cs).
// NotSet is neither a credit nor a debit.
export const isCredit = (transactionType: TransactionType) => transactionType === "Credit";

export const isDebit = (transactionType: TransactionType) => transactionType === "Debit";

export const getSplitTotal = (splits: TransactionSplit[]) => splits.reduce((total, split) => total + Number(split.amount), 0);
