import type { SortDirection } from "@andrewmclachlan/moo-ds";

import type { TransactionsFilter, transactionTypeFilter } from "models/transactions";
import { getDateRange } from "hooks/dateRange";
import { endOfDayISO, formatISODate, startOfDayISO, toDateParam } from "utils/dateFns";

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

    if (typeof search.start === "string" && search.start) result.start = toDateParam(search.start);
    if (typeof search.end === "string" && search.end) result.end = toDateParam(search.end);

    if (typeof search.sortField === "string" && search.sortField) result.sortField = search.sortField;
    if (search.sortDirection === "Ascending" || search.sortDirection === "Descending") result.sortDirection = search.sortDirection;

    return result;
};

// Reads a JSON-encoded localStorage value (the shape moo-ds `useLocalStorage` writes), returning
// the fallback when the key is absent or unparseable.
const readStored = <T>(key: string, fallback: T): T => {
    try {
        const raw = localStorage.getItem(key);
        return raw === null ? fallback : (JSON.parse(raw) as T);
    } catch {
        return fallback;
    }
};

// The persisted page-size preference (localStorage), matching useTransactionSearch's default.
export const getStoredPageSize = (): number => readStored<number>("transactions-page-size", 50);

// Fills a validated search with the same defaults the filter panel seeds from — persisted
// localStorage filters, and the default period (getDateRange: URL ?period → the stored date range →
// last month). This lets the transaction query be built synchronously (with a date range, so it
// is enabled) on the first render and warmed by the route loader, instead of only after the
// panel's post-mount effect writes the params to the URL. It mirrors useFilterPanel's
// URL-first-then-localStorage merge, including the widget-filter special-casing (an incoming
// tag/type/tagged param suppresses the stored "tagged" and description defaults).
export const resolveTransactionSearch = (rawSearch: Record<string, unknown>): TransactionSearch => {
    const search = validateTransactionSearch(rawSearch);
    const hasWidgetFilter = !!(search.tags?.length || search.type || search.tagged);

    const storedTags = readStored<number[]>("filter-tag", []);
    const tags = search.tags ?? (storedTags.length ? storedTags : undefined);

    const tagged = search.tags?.length
        ? undefined // widget tag filters always show tagged transactions
        : (search.tagged ?? (hasWidgetFilter ? undefined : readStored("filter-tagged", false) || undefined));

    const netZero = search.netZero ?? (readStored("filter-netzero", false) || undefined);
    const type = search.type ?? (readStored<transactionTypeFilter>("filter-type", "") || undefined);
    const description = hasWidgetFilter ? undefined : (readStored("filter-description", "") || undefined);

    const period = getDateRange();

    return {
        ...search,
        tags,
        tagged,
        netZero,
        type,
        description,
        start: search.start ?? formatISODate(period.startDate),
        end: search.end ?? formatISODate(period.endDate),
    };
};

// Projects the URL search state onto the filter shape consumed by the transaction query hooks.
export const searchToFilter = (search: TransactionSearch): TransactionsFilter => ({
    description: search.description,
    filterTagged: search.tagged ?? false,
    filterNetZero: search.netZero ?? false,
    transactionType: search.type ?? "",
    tags: search.tags ?? null,
    start: search.start && startOfDayISO(search.start),
    end: search.end && endOfDayISO(search.end),
});
