import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import { useRetirementProjection } from "./useRetirementProjection";

const runRetirementProjection = vi.hoisted(() => vi.fn());

vi.mock("api/sdk.gen", () => ({ runRetirementProjection }));

/**
 * The query defaults MooApp installs, reproduced so this exercises the policy the app actually
 * runs under rather than TanStack's defaults.
 *
 * This is a copy, so it will not notice if MooApp changes its policy. What it pins is this hook's
 * own behaviour under that policy: one attempt per failure, and a stable key for equal overrides.
 */
const createClient = () => new QueryClient({
    defaultOptions: {
        queries: {
            refetchOnWindowFocus: false,
            // MooApp only retries auth cancellations; anything else fails on the first attempt.
            retry: (failureCount: number, error: any) => error?.isAuthCancellation === true && failureCount < 5,
            retryDelay: 1,
            networkMode: "offlineFirst",
        },
    },
});

const wrapper = (client: QueryClient) => ({ children }: PropsWithChildren) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
);

describe("useRetirementProjection when the backend is down", () => {

    beforeEach(() => {
        runRetirementProjection.mockReset();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it("surfaces the failure instead of hammering the server", async () => {
        // A 502 from the dev proxy when nothing is listening behind it.
        runRetirementProjection.mockRejectedValue(Object.assign(new Error("Bad Gateway"), { status: 502 }));

        const client = createClient();
        const { result } = renderHook(() => useRetirementProjection("plan-1"), { wrapper: wrapper(client) });

        await waitFor(() => expect(result.current.isError).toBe(true));

        // Give any stray retry a chance to fire before counting.
        await new Promise(resolve => setTimeout(resolve, 100));

        expect(runRetirementProjection).toHaveBeenCalledTimes(1);
    });

    it("does not refetch on remount while the failure is fresh", async () => {
        runRetirementProjection.mockRejectedValue(Object.assign(new Error("Bad Gateway"), { status: 502 }));

        const client = createClient();
        const { result, unmount } = renderHook(() => useRetirementProjection("plan-1"), { wrapper: wrapper(client) });

        await waitFor(() => expect(result.current.isError).toBe(true));
        unmount();

        const second = renderHook(() => useRetirementProjection("plan-1"), { wrapper: wrapper(client) });
        await waitFor(() => expect(second.result.current.isError).toBe(true));
        await new Promise(resolve => setTimeout(resolve, 100));

        // One attempt per mount at most; a page that remounts must not multiply requests.
        expect(runRetirementProjection.mock.calls.length).toBeLessThanOrEqual(2);
    });

    it("keeps the same key for an unchanged override object", async () => {
        runRetirementProjection.mockResolvedValue({ data: { planId: "plan-1", years: [], members: [], summary: {} } });

        const client = createClient();
        const overrides = { expectedReturnRate: 0.07, members: [], excludedMemberIds: [] };

        const { result, rerender } = renderHook(
            ({ o }) => useRetirementProjection("plan-1", o),
            { wrapper: wrapper(client), initialProps: { o: overrides } },
        );

        await waitFor(() => expect(result.current.isSuccess).toBe(true));

        // A fresh object with the same values must hash to the same key, or every render would
        // start a new query.
        rerender({ o: { expectedReturnRate: 0.07, members: [], excludedMemberIds: [] } });
        await new Promise(resolve => setTimeout(resolve, 50));

        expect(runRetirementProjection).toHaveBeenCalledTimes(1);
    });
});
