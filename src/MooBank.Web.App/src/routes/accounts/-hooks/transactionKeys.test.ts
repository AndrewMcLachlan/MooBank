import { describe, it, expect } from "vitest";
import { QueryClient } from "@tanstack/react-query";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
    getAccountsQueryKey,
    getAccountQueryKey,
    inOutReportQueryKey,
    getTagsQueryKey,
} from "api/@tanstack/react-query.gen";
import { invalidateTransactionLists, invalidateAccountViews } from "./transactionKeys";

// The generated query keys are single-element arrays of `{ _id, baseURL, path, query }`, so the
// id-only partial keys these helpers use should match every seeded variant via TanStack Query's
// partial deep matching. We seed real generated keys and assert which queries get invalidated —
// this exercises the actual matching, not just the literal key shape.

const seed = (queryClient: QueryClient, queryKey: unknown[]) =>
    queryClient.setQueryData(queryKey, { seeded: true });

const isInvalidated = (queryClient: QueryClient, queryKey: unknown[]) =>
    queryClient.getQueryState(queryKey)?.isInvalidated === true;

describe("invalidateTransactionLists", () => {
    it("invalidates every transaction list query regardless of account, page, filter and sort", async () => {
        const queryClient = new QueryClient();

        const listA = getTransactionsQueryKey({ path: { instrumentId: "acc-1", pageSize: 50, pageNumber: 1 }, query: { SortDirection: "Descending" } });
        const listB = getTransactionsQueryKey({ path: { instrumentId: "acc-2", pageSize: 20, pageNumber: 3 }, query: { SortDirection: "Ascending", Filter: "coffee" } });
        const untagged = getUntaggedTransactionsQueryKey({ path: { instrumentId: "acc-1", pageSize: 50, pageNumber: 1 }, query: { SortDirection: "Descending" } });
        seed(queryClient, listA);
        seed(queryClient, listB);
        seed(queryClient, untagged);

        await invalidateTransactionLists(queryClient);

        expect(isInvalidated(queryClient, listA)).toBe(true);
        expect(isInvalidated(queryClient, listB)).toBe(true);
        expect(isInvalidated(queryClient, untagged)).toBe(true);
    });

    it("does not invalidate account or reference-data queries", async () => {
        const queryClient = new QueryClient();

        const accounts = getAccountsQueryKey();
        const tags = getTagsQueryKey();
        seed(queryClient, accounts);
        seed(queryClient, tags);

        await invalidateTransactionLists(queryClient);

        expect(isInvalidated(queryClient, accounts)).toBe(false);
        expect(isInvalidated(queryClient, tags)).toBe(false);
    });
});

describe("invalidateAccountViews", () => {
    it("invalidates the accounts list, single-account and in/out report queries", async () => {
        const queryClient = new QueryClient();

        const accounts = getAccountsQueryKey();
        const account = getAccountQueryKey({ path: { instrumentId: "acc-1" } });
        const inOut = inOutReportQueryKey({ path: { accountId: "acc-1", start: "2026-01-01", end: "2026-12-31" } });
        seed(queryClient, accounts);
        seed(queryClient, account);
        seed(queryClient, inOut);

        await invalidateAccountViews(queryClient);

        expect(isInvalidated(queryClient, accounts)).toBe(true);
        expect(isInvalidated(queryClient, account)).toBe(true);
        expect(isInvalidated(queryClient, inOut)).toBe(true);
    });

    it("does not invalidate transaction list or reference-data queries", async () => {
        const queryClient = new QueryClient();

        const list = getTransactionsQueryKey({ path: { instrumentId: "acc-1", pageSize: 50, pageNumber: 1 }, query: { SortDirection: "Descending" } });
        const tags = getTagsQueryKey();
        seed(queryClient, list);
        seed(queryClient, tags);

        await invalidateAccountViews(queryClient);

        expect(isInvalidated(queryClient, list)).toBe(false);
        expect(isInvalidated(queryClient, tags)).toBe(false);
    });
});
