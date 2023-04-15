export interface State {
    app: App,
    transactions: Transactions,
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
    sortDirection: sortDirection;
}

export interface TransactionsFilter {
    filterTagged?: boolean;
    description?: string;
    tag: number | null;
    start?: string;
    end?: string;
}

export type sortDirection = "Ascending" | "Descending";
