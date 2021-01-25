export interface State {
    app: App,
    transactions?: Transactions,
}

export interface App {
    baseUrl: string;
    message?: string;
}

export interface Transactions {
    currentPage: number;
    pageSize: number;
    filterTagged: boolean;
    sortField: string;
    sortDirection: sortDirection;
}

export type sortDirection = "Ascending" | "Descending";
