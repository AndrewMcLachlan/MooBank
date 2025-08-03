import { SortDirection } from "@andrewmclachlan/mooapp";

export interface State {
    app: App,
    transactions: Transactions,
    stockTransactions: Transactions,
}

export interface App {
    baseUrl: string;
    message?: string;
}

export interface Transactions {
    currentPage: number;
    pageSize: number;
    filter: TransactionsFilter;
    sortField: string;
    sortDirection: SortDirection;
}

export interface TransactionsFilter {
    filterTagged?: boolean;
    filterNetZero?: boolean;
    description?: string;
    transactionType: transactionTypeFilter;
    tags?: number[] | null;
    start?: string;
    end?: string;
}

export type transactionTypeFilter = "" | "Debit" | "Credit";
