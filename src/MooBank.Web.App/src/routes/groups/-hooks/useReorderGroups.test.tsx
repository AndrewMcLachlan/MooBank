import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import { getAllGroupsQueryKey } from "api/@tanstack/react-query.gen";
import { useReorderGroups } from "./useReorderGroups";

const reorderGroups = vi.hoisted(() => vi.fn());

vi.mock("api/sdk.gen", () => ({ reorderGroups }));
vi.mock("@andrewmclachlan/moo-ds", () => ({ toast: { error: vi.fn() } }));

const group = (id: string, name: string) => ({ id, name, showTotal: false });

const seeded = () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
    client.setQueryData(getAllGroupsQueryKey(), [group("a", "Alpha"), group("b", "Beta"), group("c", "Gamma")]);
    return client;
};

const wrapper = (client: QueryClient) => ({ children }: PropsWithChildren) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
);

const names = (client: QueryClient) =>
    (client.getQueryData(getAllGroupsQueryKey()) as { name: string }[]).map(g => g.name);

describe("useReorderGroups", () => {

    beforeEach(() => {
        reorderGroups.mockReset();
    });

    /**
     * A drag has to look like it worked the moment it is dropped. Waiting for the round trip means
     * the row springs back to where it was and then jumps forward again, which reads as a bug.
     */
    it("puts the rows in their new order before the request comes back", async () => {
        const client = seeded();
        let release: (value: unknown) => void = () => { };
        reorderGroups.mockReturnValue(new Promise(resolve => { release = resolve; }));

        const { result } = renderHook(() => useReorderGroups(), { wrapper: wrapper(client) });

        act(() => { result.current.reorder(["c", "a", "b"]); });

        await waitFor(() => expect(names(client)).toEqual(["Gamma", "Alpha", "Beta"]));
        expect(reorderGroups).toHaveBeenCalled();

        release({ data: [group("c", "Gamma"), group("a", "Alpha"), group("b", "Beta")] });
    });

    /**
     * The order the server stored is the one that counts — it is what a reload will show.
     */
    it("takes the order the server reports back", async () => {
        const client = seeded();
        reorderGroups.mockResolvedValue({ data: [group("b", "Beta"), group("c", "Gamma"), group("a", "Alpha")] });

        const { result } = renderHook(() => useReorderGroups(), { wrapper: wrapper(client) });

        await act(async () => { await result.current.reorder(["c", "a", "b"]); });

        expect(names(client)).toEqual(["Beta", "Gamma", "Alpha"]);
    });

    /**
     * Whatever the drag looked like, a failed save has to leave the list saying what is actually
     * stored, or the next reload silently contradicts the screen.
     */
    it("puts the old order back when the save fails", async () => {
        const client = seeded();
        reorderGroups.mockRejectedValue(new Error("nope"));

        const { result } = renderHook(() => useReorderGroups(), { wrapper: wrapper(client) });

        await act(async () => {
            await result.current.reorder(["c", "a", "b"]).catch(() => { });
        });

        await waitFor(() => expect(names(client)).toEqual(["Alpha", "Beta", "Gamma"]));
    });
});
