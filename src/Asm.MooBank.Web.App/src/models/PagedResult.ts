export interface PagedResult<T> {
    results: T[],
    total: number;
}