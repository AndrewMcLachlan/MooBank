import type { SortDirection } from "@andrewmclachlan/moo-ds";

import type { TransactionsFilter } from "models/transactions";
import { endOfDayISO, startOfDayISO, toDateParam } from "utils/dateFns";
import { parsePage, parseSortDirection, type GridSearch } from "utils/gridSearch";

// URL-driven state for the stock-transaction list. Intentionally narrower than the account
// transaction list (see routes/accounts/-transactions/transactionSearch.ts): the stock list only
// filters by description and period — no tag/type/net-zero filtering — matching the behaviour of
// the former StockTransactions Redux slice.
export interface StockTransactionSearch extends Omit<GridSearch, "search" | "pageSize"> {
    description?: string;
    start?: string;
    end?: string;
}

export const defaultStockSortField = "TransactionDate";
export const defaultStockSortDirection: SortDirection = "Descending";

export const validateStockTransactionSearch = (search: Record<string, unknown>): StockTransactionSearch => {
    const result: StockTransactionSearch = {};

    const page = parsePage(search.page);
    if (page) result.page = page;

    if (typeof search.description === "string" && search.description) result.description = search.description;
    if (typeof search.start === "string" && search.start) result.start = toDateParam(search.start);
    if (typeof search.end === "string" && search.end) result.end = toDateParam(search.end);

    if (typeof search.sortField === "string" && search.sortField && search.sortField !== defaultStockSortField) result.sortField = search.sortField;

    const sortDirection = parseSortDirection(search.sortDirection);
    if (sortDirection && sortDirection !== defaultStockSortDirection) result.sortDirection = sortDirection;

    return result;
};

export const stockSearchToFilter = (search: StockTransactionSearch): TransactionsFilter => ({
    description: search.description,
    // The stock list always resets transactionType to "" (a deliberate divergence carried over
    // from the former StockTransactions slice).
    transactionType: "",
    start: search.start && startOfDayISO(search.start),
    end: search.end && endOfDayISO(search.end),
});
