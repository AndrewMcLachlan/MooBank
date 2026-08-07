import type { SortDirection } from "@andrewmclachlan/moo-ds";

import type { TransactionsFilter } from "models/transactions";

// URL-driven state for the stock-transaction list. Intentionally narrower than the account
// transaction list (see routes/accounts/-transactions/transactionSearch.ts): the stock list only
// filters by description and period — no tag/type/net-zero filtering — matching the behaviour of
// the former StockTransactions Redux slice.
export interface StockTransactionSearch {
    page?: number;
    description?: string;
    start?: string;
    end?: string;
    sortField?: string;
    sortDirection?: SortDirection;
}

export const defaultStockSortField = "TransactionDate";
export const defaultStockSortDirection: SortDirection = "Descending";

export const validateStockTransactionSearch = (search: Record<string, unknown>): StockTransactionSearch => {
    const result: StockTransactionSearch = {};

    const page = Number(search.page);
    if (Number.isFinite(page) && page > 1) result.page = page;

    if (typeof search.description === "string" && search.description) result.description = search.description;
    if (typeof search.start === "string" && search.start) result.start = search.start;
    if (typeof search.end === "string" && search.end) result.end = search.end;

    if (typeof search.sortField === "string" && search.sortField) result.sortField = search.sortField;
    if (search.sortDirection === "Ascending" || search.sortDirection === "Descending") result.sortDirection = search.sortDirection;

    return result;
};

export const stockSearchToFilter = (search: StockTransactionSearch): TransactionsFilter => ({
    description: search.description,
    // The stock list always resets transactionType to "" (a deliberate divergence carried over
    // from the former StockTransactions slice).
    transactionType: "",
    start: search.start,
    end: search.end,
});
