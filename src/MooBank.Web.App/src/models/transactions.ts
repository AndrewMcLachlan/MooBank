import type { TransactionSplit } from "api/types.gen";

export type transactionTypeFilter = "" | "Debit" | "Credit";

export const toTransactionTypeFilter = (value: unknown): transactionTypeFilter =>
    value === "Debit" || value === "Credit" ? value : "";

export const transactionTypeFilterLabels: Record<Exclude<transactionTypeFilter, "">, string> = {
    Credit: "Income",
    Debit: "Expense",
};

// The resolved set of transaction-list filters. Sourced from the route search params
// (see routes/accounts/-transactions/transactionSearch.ts); replaces the former Redux slice state.
export interface TransactionsFilter {
    filterTagged?: boolean;
    filterNetZero?: boolean;
    description?: string;
    transactionType: transactionTypeFilter;
    tags?: number[] | null;
    start?: string;
    end?: string;
}

export interface TransactionUpdate {
    excludeFromReporting: boolean;
    notes?: string;
    splits?: TransactionSplit[];
}

// Each unsaved split needs a unique id so React keys and edit/remove operations
// don't collide. The API uses the client-supplied id when creating new splits.
export const newTransactionSplit = (): TransactionSplit => ({
    id: crypto.randomUUID(),
    amount: 0,
    offsetBy: [],
    tags: [],
});

export interface CreateTransaction {
    amount: number;
    description: string;
    reference?: string;
    transactionTime: string;
}
