import type { SortDirection } from "@andrewmclachlan/moo-ds";

import { toTransactionTypeFilter, type TransactionsFilter, type transactionTypeFilter } from "models/transactions";
import { getDateRange } from "hooks/dateRange";
import type { Period } from "models/dateFns";
import { endOfDayISO, formatISODate, startOfDayISO, toDateParam } from "utils/dateFns";
import { parsePage, parsePageSize, parseSortDirection, type GridSearch } from "utils/gridSearch";

// Typed, URL-driven state for the transaction list: filter, sort, page and page size live in the
// route search params so the view is shareable/bookmarkable. Without a pageSize param, the
// persisted preference (localStorage) applies.
export interface TransactionSearch extends Omit<GridSearch, "search"> {
    description?: string;
    /** Show only untagged transactions (was filterTagged). */
    tagged?: boolean;
    /** Exclude fully offset transactions (was filterNetZero). */
    netZero?: boolean;
    type?: transactionTypeFilter;
    tags?: number[];
    start?: string;
    end?: string;
}

export const transactionsPageSizeKey = "transactions-page-size";
export const defaultTransactionsPageSize = 50;
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

    const page = parsePage(search.page);
    if (page) result.page = page;

    const pageSize = parsePageSize(search.pageSize);
    if (pageSize) result.pageSize = pageSize;

    if (typeof search.description === "string" && search.description) result.description = search.description;

    if (isTruthy(search.tagged) || isTruthy(search.untagged)) result.tagged = true;
    if (isTruthy(search.netZero) || isTruthy(search.netzero)) result.netZero = true;

    const type = toTransactionTypeFilter(search.type);
    if (type) result.type = type;

    const tags = parseTags(search.tags ?? search.tag);
    if (tags) result.tags = tags;

    if (typeof search.start === "string" && search.start) result.start = toDateParam(search.start);
    if (typeof search.end === "string" && search.end) result.end = toDateParam(search.end);

    if (typeof search.sortField === "string" && search.sortField && search.sortField !== defaultSortField) result.sortField = search.sortField;

    const sortDirection = parseSortDirection(search.sortDirection);
    if (sortDirection && sortDirection !== defaultSortDirection) result.sortDirection = sortDirection;

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
export const getStoredPageSize = (): number =>
    parsePageSize(readStored<number>(transactionsPageSizeKey, defaultTransactionsPageSize)) ?? defaultTransactionsPageSize;

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
    const type = search.type ?? (toTransactionTypeFilter(readStored<unknown>("filter-type", "")) || undefined);
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

// The start/end params for a period, or none while the period is still unknown.
export const periodSearch = (period: Period): Pick<TransactionSearch, "start" | "end"> =>
    period?.startDate && period?.endDate
        ? { start: formatISODate(period.startDate), end: formatISODate(period.endDate) }
        : {};

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
