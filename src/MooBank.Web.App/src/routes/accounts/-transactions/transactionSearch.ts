import type { SortDirection } from "@andrewmclachlan/moo-ds";

import type { TransactionsFilter, transactionTypeFilter } from "models/transactions";

// Typed, URL-driven state for the transaction list. Replaces the former Redux slice: filter,
// sort and page live in the route search params so the view is shareable/bookmarkable. pageSize
// is a persisted preference (localStorage) rather than URL state.
export interface TransactionSearch {
    page?: number;
    description?: string;
    /** Show only untagged transactions (was filterTagged). */
    tagged?: boolean;
    /** Exclude fully offset transactions (was filterNetZero). */
    netZero?: boolean;
    type?: transactionTypeFilter;
    tags?: number[];
    start?: string;
    end?: string;
    sortField?: string;
    sortDirection?: SortDirection;
}

export const defaultSortField = "TransactionTime";
export const defaultSortDirection: SortDirection = "Descending";

const isTruthy = (value: unknown): boolean =>
    value === true || value === "true" || value === 1 || value === "1";

const parseTags = (value: unknown): number[] | undefined => {
    const source = Array.isArray(value)
        ? value
        : typeof value === "string"
            ? value.split(",")
            : value === undefined || value === null
                ? []
                : [value];

    const numbers = source.map(Number).filter((n) => Number.isFinite(n));
    return numbers.length ? numbers : undefined;
};

// Validates and normalises the raw search object. Also accepts the legacy dashboard-widget
// param spellings (?untagged, ?netzero, ?tag=<id>) so cross-feature links keep working.
export const validateTransactionSearch = (search: Record<string, unknown>): TransactionSearch => {
    const result: TransactionSearch = {};

    const page = Number(search.page);
    if (Number.isFinite(page) && page > 1) result.page = page;

    if (typeof search.description === "string" && search.description) result.description = search.description;

    if (isTruthy(search.tagged) || isTruthy(search.untagged)) result.tagged = true;
    if (isTruthy(search.netZero) || isTruthy(search.netzero)) result.netZero = true;

    if (search.type === "Debit" || search.type === "Credit") result.type = search.type;

    const tags = parseTags(search.tags ?? search.tag);
    if (tags) result.tags = tags;

    if (typeof search.start === "string" && search.start) result.start = search.start;
    if (typeof search.end === "string" && search.end) result.end = search.end;

    if (typeof search.sortField === "string" && search.sortField) result.sortField = search.sortField;
    if (search.sortDirection === "Ascending" || search.sortDirection === "Descending") result.sortDirection = search.sortDirection;

    return result;
};

// Projects the URL search state onto the filter shape consumed by the transaction query hooks.
export const searchToFilter = (search: TransactionSearch): TransactionsFilter => ({
    description: search.description,
    filterTagged: search.tagged ?? false,
    filterNetZero: search.netZero ?? false,
    transactionType: search.type ?? "",
    tags: search.tags ?? null,
    start: search.start,
    end: search.end,
});
