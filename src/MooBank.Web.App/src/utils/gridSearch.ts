import type { SortDirection } from "@andrewmclachlan/moo-ds";

export interface GridSearch {
    page?: number;
    pageSize?: number;
    search?: string;
    sortField?: string;
    sortDirection?: SortDirection;
}

export interface GridSearchDefaults {
    sortField?: string;
    sortDirection?: SortDirection;
    sortFields?: readonly string[];
}

export const pageSizes = [10, 20, 50, 100];

export const parsePage = (value: unknown): number | undefined => {
    const page = Number(value);
    return Number.isInteger(page) && page > 1 ? page : undefined;
};

export const parsePageSize = (value: unknown): number | undefined => {
    const pageSize = Number(value);
    return pageSizes.includes(pageSize) ? pageSize : undefined;
};

export const parseSortDirection = (value: unknown): SortDirection | undefined =>
    value === "Ascending" || value === "Descending" ? value : undefined;

export const validatePageSearch = (search: Record<string, unknown>): Pick<GridSearch, "page"> => {
    const page = parsePage(search.page);
    return page ? { page } : {};
};

export const validateGridSearch = (search: Record<string, unknown>, defaults: GridSearchDefaults = {}): GridSearch => {
    const result: GridSearch = {};

    const page = parsePage(search.page);
    if (page) result.page = page;

    const pageSize = parsePageSize(search.pageSize);
    if (pageSize) result.pageSize = pageSize;

    if (typeof search.search === "string" && search.search) result.search = search.search;

    const sortField = search.sortField;
    if (typeof sortField === "string" && sortField !== defaults.sortField && (!defaults.sortFields || defaults.sortFields.includes(sortField))) {
        result.sortField = sortField;
    }

    const sortDirection = parseSortDirection(search.sortDirection);
    if (sortDirection && sortDirection !== defaults.sortDirection) result.sortDirection = sortDirection;

    return result;
};

const normalise = (value: unknown): string | undefined => {
    if (value === undefined || value === null || value === "" || value === false) return undefined;
    if (Array.isArray(value)) return value.length ? value.join(",") : undefined;
    return String(value);
};

export const isSearchChange = (current: object, values: object): boolean =>
    Object.entries(values).some(([key, value]) => normalise(value) !== normalise((current as Record<string, unknown>)[key]));

export const clampPage = (page: number, numberOfPages: number): number =>
    Math.max(1, Math.min(page, numberOfPages));
